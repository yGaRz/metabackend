package mmf

import (
	"errors"
	"fmt"
	"log"
	"time"

	"google.golang.org/protobuf/proto"
	"google.golang.org/protobuf/types/known/anypb"
	"google.golang.org/protobuf/types/known/wrapperspb"
	"open-match.dev/open-match/pkg/pb"
	"strconv"
	"math/rand"
)

const (
	NumberOfPlayersLabel = "numberOfPlayers"
	FreeSlotsLabel       = "freeSlots"
)

func GetNumberOfPlayersFromMatchProfile(mp *pb.MatchProfile) (int32, error) {
	extensions := mp.Extensions
	if extensions == nil {
		return 0, errors.New("cannot get number of players from match profile: extensions do not exist")
	}
	var numberOfPlayers wrapperspb.Int32Value
	err := anypb.UnmarshalTo(extensions[NumberOfPlayersLabel], &numberOfPlayers, proto.UnmarshalOptions{})
	if err != nil {
		return 0, err
	}
	return numberOfPlayers.Value, nil
}

func SetBackfillFreeSlots(backfill *pb.Backfill, numberOfSlots int32) error {
	freeSlots, err := anypb.New(wrapperspb.Int32(numberOfSlots))
	if err != nil {
		return err
	}
	backfill.Extensions[FreeSlotsLabel] = freeSlots
	return nil
}

func GetBackfillFreeSlots(backfill *pb.Backfill) (int32, error) {
	extensions := backfill.Extensions
	if extensions == nil {
		return 0, errors.New("cannot get free slots from backfill: extensions do not exist")
	}
	var freeSlots wrapperspb.Int32Value
	err := anypb.UnmarshalTo(extensions[FreeSlotsLabel], &freeSlots, proto.UnmarshalOptions{})
	if err != nil {
		return 0, err
	}
	return freeSlots.Value, nil
}

func MakeMatch(mp *pb.MatchProfile, matchTickets []*pb.Ticket, backfill *pb.Backfill, poolName string, matchIndex int, isAllocateNew bool) (*pb.Match, error) {
	now := time.Now().Format(time.RFC3339)
    randomValue := strconv.Itoa(rand.Intn(100000))
    matchID := fmt.Sprintf("%s-%s-%s-%s", mp.GetName(), mp.GetPools()[0].GetName(), now, randomValue)

	match := &pb.Match{
		MatchId:            matchID,
		MatchProfile:       mp.Name,
		MatchFunction:      "basic-matchfunction",
		Tickets:            matchTickets,
		Extensions:         map[string]*anypb.Any{},
		Backfill:           backfill,
		AllocateGameserver: isAllocateNew,
	}
	printMatch(match)
	return match, nil
}

func printMatch(match *pb.Match) {
	str := ""
	for i, ticket := range match.Tickets {
		if i > 0 {
			str += ", "
		}
		str += ticket.Id
	}
	log.Printf("A new match: [%s], backfillId=%s, isAllocateNew=%t", str, match.Backfill.Id, match.AllocateGameserver)
}
