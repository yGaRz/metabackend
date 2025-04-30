package test

import (
	director "metacity-director/internal"
	"testing"

	"github.com/stretchr/testify/assert"
	"google.golang.org/protobuf/types/known/anypb"
	"google.golang.org/protobuf/types/known/wrapperspb"
	"open-match.dev/open-match/pkg/pb"
)

const (
	matchProfileName = "testProfile"
	matchId          = "match007"
	numberOfPlayers  = 5
)

var ticketIds = []string{"ticket1", "ticket2", "ticket3"}

// The most basic test that check that there is no error or panic
func TestGenerateMatchProfiles(t *testing.T) {
	profiles, err := director.GenerateProfiles()
	assert.Nilf(t, err, "Error is not nil")
	assert.Equal(t, 2, len(profiles), "Unexpected amount of match profiles")
}

// The most basic test that check that there is no error or panic
func TestAssign(t *testing.T) {
	mockBe := pb.BackendServiceClient(nil)
	director.CallAssignTickets = func(be pb.BackendServiceClient, ticketIDs []string, gameServerUrl string) error {
		return nil
	}

	director.SetDevelopmentMode()
	matchProfile := makeMatchProfile()
	match := makeMatch(false)
	err := director.Assign(mockBe, match, matchProfile)
	assert.Nilf(t, err, "Error is not nil")
}

func makeMatch(isAllocateNew bool) *pb.Match {
	tickets := []*pb.Ticket{}
	for _, id := range ticketIds {
		tickets = append(tickets, makeTicket(id))
	}
	var extensions = map[string]*anypb.Any{}

	return &pb.Match{
		MatchId:            matchId,
		MatchProfile:       matchProfileName,
		Extensions:         extensions,
		Tickets:            tickets,
		AllocateGameserver: isAllocateNew,
	}
}

func makeTicket(id string) *pb.Ticket {
	return &pb.Ticket{
		Id: id,
	}
}

func makeMatchProfile() *pb.MatchProfile {
	extensions := make(map[string]*anypb.Any)
	numberOfPlayersField, err := anypb.New(wrapperspb.Int32(numberOfPlayers))
	if err != nil {
		panic(err)
	}
	extensions["number_of_players"] = numberOfPlayersField
	return &pb.MatchProfile{
		Name: "match_profile_bronx",
		Pools: []*pb.Pool{
			{
				Name: "pool_bronx",
				TagPresentFilters: []*pb.TagPresentFilter{
					{
						Tag: "bronx",
					},
				},
			},
		},
		Extensions: extensions,
	}
}
