package director

import (
    "context"
    "fmt"
    "io"
    "log"
    "math/rand"
    "os"
    "strconv"
    "sync"
    "time"
    "strings"

    agonesv1 "agones.dev/agones/pkg/apis/agones/v1"
    allocationv1 "agones.dev/agones/pkg/apis/allocation/v1"
    "agones.dev/agones/pkg/client/clientset/versioned"
    "google.golang.org/grpc"
    metav1 "k8s.io/apimachinery/pkg/apis/meta/v1"
    "k8s.io/client-go/rest"
    "open-match.dev/open-match/pkg/pb"
)

const (
    omBackendEndpoint  = "open-match-backend.open-match.svc.cluster.local:50505"
    omFrontendEndpoint = "open-match-frontend.open-match.svc.cluster.local:50504"
    profileEnv         = "PROFILE"
    jsonContentType    = "application/json"
    mmfHostName        = "metacity-matchfunction.metacity.svc.cluster.local"
    mmfPort            = 50502
)

var (
    agonesClient  *versioned.Clientset
    isDevelopment = false
    fe            pb.FrontendServiceClient
)

func Start() {
    // Check environment variables
    profile, present := os.LookupEnv(profileEnv)
    if present && profile == "dev" {
        SetDevelopmentMode()
    }

    // Connect to Open Match Backend.
    beConn, err := grpc.Dial(omBackendEndpoint, grpc.WithInsecure())
    if err != nil {
        panic(fmt.Sprintf("Failed to connect to Open Match Backend, got %s", err.Error()))
    }
    defer beConn.Close()
    be := pb.NewBackendServiceClient(beConn)

    // Connect to Open Match Frontend.
    feConn, err := grpc.Dial(omFrontendEndpoint, grpc.WithInsecure())
    if err != nil {
        panic(fmt.Sprintf("Failed to connect to Open Match Frontend, got %s", err.Error()))
    }
    defer feConn.Close()
    fe = pb.NewFrontendServiceClient(feConn)

    agonesClient = getAgonesClient()

    // Generate the profiles to fetch matches for.
    profiles, err := GenerateProfiles()
    if err != nil {
        panic(err)
    }

    log.Printf("Fetching matches for %v profiles", len(profiles))
    for range time.Tick(time.Second * 5) {
        // Fetch matches for each profile and make assignments for Tickets in
        // the matches returned.
        var wg sync.WaitGroup
        for _, p := range profiles {
            wg.Add(1)
            go func(wg *sync.WaitGroup, p *pb.MatchProfile) {
                defer wg.Done()
                matches, err := fetch(be, p)
                if err != nil {
                    log.Printf("Failed to fetch matches for profile %v, got %s", p.GetName(), err.Error())
                    return
                }

                log.Printf("Generated %v matches for profile %v", len(matches), p.GetName())
                assignMatches(be, matches, p)
            }(&wg, p)
        }

        wg.Wait()
    }
}

// initialize kubernetes-API client
func getAgonesClient() *versioned.Clientset {
    if isDevelopment {
        return nil
    }
    config, err := rest.InClusterConfig()
    if err != nil {
        log.Printf("Could not create in cluster config: %s", err)
        panic(err)
    }

    agonesClient, err := versioned.NewForConfig(config)
    if err != nil {
        log.Printf("Could not create agones client: %s", err)
        panic(err)
    }
    log.Printf("Created the agones api clientset")
    return agonesClient
}

func fetch(be pb.BackendServiceClient, p *pb.MatchProfile) ([]*pb.Match, error) {
    req := &pb.FetchMatchesRequest{
        Config: &pb.FunctionConfig{
            Host: mmfHostName,
            Port: mmfPort,
            Type: pb.FunctionConfig_GRPC,
        },
        Profile: p,
    }

    stream, err := be.FetchMatches(context.Background(), req)
    if err != nil {
        log.Println(err)
        return nil, err
    }

    var result []*pb.Match
    for {
        resp, err := stream.Recv()
        if err == io.EOF {
            break
        }

        if err != nil {
            return nil, err
        }

        result = append(result, resp.GetMatch())
    }

    log.Printf("Fetched matches: %d для профиля: %s", len(result), p.GetName())
    return result, nil
}

func assignMatches(be pb.BackendServiceClient, matches []*pb.Match, profile *pb.MatchProfile) {
    for _, match := range matches {
        err := Assign(be, match, profile)
        if err != nil {
            log.Printf("Failed to assign servers to match %v, got %s", match.GetMatchId(), err.Error())
        }
    }
}

func Assign(be pb.BackendServiceClient, match *pb.Match, profile *pb.MatchProfile) error {
    ticketIDs := []string{}
    for _, t := range match.GetTickets() {
        ticketIDs = append(ticketIDs, t.Id)
    }
    gameServerType := profile.Pools[0].TagPresentFilters[0].Tag
    gameServerUrl, err := allocate(gameServerType, len(match.Tickets), match)
    if err != nil {
        return fmt.Errorf("failed to allocate game server for map type %v, got error %w", gameServerType, err)
    }

    // Назначаем билеты
    err = CallAssignTickets(be, ticketIDs, gameServerUrl)
    if err != nil {
        return fmt.Errorf("AssignTickets failed for match %v, got %w", match.GetMatchId(), err)
    }

    log.Printf("Assigned server %v to match %v", gameServerUrl, match.GetMatchId())
    return nil
}

func SetDevelopmentMode() {
    isDevelopment = true
}

var CallAssignTickets = func(be pb.BackendServiceClient, ticketIDs []string, gameServerUrl string) error {
    req := &pb.AssignTicketsRequest{
        Assignments: []*pb.AssignmentGroup{
            {
                TicketIds: ticketIDs,
                Assignment: &pb.Assignment{
                    Connection: gameServerUrl,
                },
            },
        },
    }

    _, err := be.AssignTickets(context.Background(), req)
    return err
}

func allocate(gameserverType string, numPlayers int, match *pb.Match) (string, error) {
    log.Printf("Allocating game server %s for %d players", gameserverType, numPlayers)
    if isDevelopment {
        return generateRandomUrl(), nil
    }

    fleetName := "neodao-" + strings.ToLower(gameserverType)
	var gameserverStateAlloc agonesv1.GameServerState = agonesv1.GameServerStateAllocated
    var gameserverStateReady agonesv1.GameServerState = agonesv1.GameServerStateReady

    gsa := &allocationv1.GameServerAllocation{ObjectMeta: metav1.ObjectMeta{GenerateName: fleetName + "-allocation-", Namespace: "default"},
		Spec: allocationv1.GameServerAllocationSpec{
			Selectors: []allocationv1.GameServerSelector{
				{
					LabelSelector: metav1.LabelSelector{
						MatchLabels: map[string]string{"agones.dev/fleet": fleetName},
					},
					GameServerState: &gameserverStateAlloc,
					Players: &allocationv1.PlayerSelector{
						MinAvailable: 1,
						MaxAvailable: 10, // TODO: its magic number, move to env
					},
				},
                {
					LabelSelector: metav1.LabelSelector{
						MatchLabels: map[string]string{"agones.dev/fleet": fleetName},
					},
					GameServerState: &gameserverStateReady,
				},
			},
		},
	}

    newAllocation, err := agonesClient.AllocationV1().GameServerAllocations("default").Create(context.TODO(), gsa, metav1.CreateOptions{})
    if err != nil {
        log.Printf("Could not create new GameServerAllocation: %s", err)
        return "", err
    }

    state := newAllocation.Status.State
    if state != allocationv1.GameServerAllocationAllocated {
        err = fmt.Errorf("wrong state of allocated game server: expected Allocated, got %s", state)
        return "", err
    }

    // Return url of the allocated GameServer
    address := newAllocation.Status.Address
    port := newAllocation.Status.Ports[0].Port
    gameServerUrl := fmt.Sprintf("%s:%d", address, port)

    log.Printf("Allocated new game server URL: %s; name: %s", gameServerUrl, newAllocation.Status.GameServerName)
    return gameServerUrl, nil
}

func generateRandomUrl() string {
    url := ""
    for i := 0; i < 4; i++ {
        if i > 0 {
            url += "."
        }
        n := rand.Intn(255) + 1
        url += strconv.Itoa(n)
    }
    port := rand.Intn(9000) + 1000
    url += ":" + strconv.Itoa(port)
    return url
}