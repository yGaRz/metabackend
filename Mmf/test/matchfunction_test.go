package mmf

import (
	mmf "metacity-mmf/internal"

	"fmt"
	"testing"

	"github.com/google/uuid"
	"github.com/stretchr/testify/assert"
	"google.golang.org/protobuf/proto"
	"google.golang.org/protobuf/types/known/anypb"
	"google.golang.org/protobuf/types/known/wrapperspb"
	"open-match.dev/open-match/pkg/pb"
)

const (
	matchProfileName       = "testProfile"
	poolName1              = "pool1"
	poolName2              = "pool2"
	numberOfPlayers  int32 = 5
)

var currentPlayerId int32
var backfillId1 = uuid.New().String()
var backfillId2 = uuid.New().String()
var matchProfile = makeMatchProfile()

// There are no tickets
// We expect to receive zero matches
func TestEmpty(t *testing.T) {
	setup()
	backfills := map[string][]*pb.Backfill{
		poolName1: {
			makeBackfill(backfillId1, 3),
		},
	}
	poolTickets := map[string][]*pb.Ticket{
		poolName1: {},
	}
	matches, err := mmf.MakeMatches(matchProfile, backfills, poolTickets)

	assert.Nilf(t, err, "Error message was not nil")
	assert.Equal(t, 0, len(matches), "The number of matches was not equal to 0")
}

// There are no backfills present, so we shall create a new one
// We expect to receive two matches: one with 5 tickets and another one with the last ticket
func TestNewBackfills(t *testing.T) {
	setup()
	backfills := map[string][]*pb.Backfill{}
	poolTickets := map[string][]*pb.Ticket{
		poolName1: {
			makeTicket(),
			makeTicket(),
			makeTicket(),
			makeTicket(),
			makeTicket(),
			makeTicket(),
		},
	}
	matches, err := mmf.MakeMatches(matchProfile, backfills, poolTickets)

	assert.Nilf(t, err, "Error message was not nil")
	assert.Equal(t, 2, len(matches), "The number of matches was not equal to 2")
	checkMatch(t, matches[0], []string{
		"ticket_1", "ticket_2", "ticket_3", "ticket_4", "ticket_5",
	}, true, "", 0)
	checkMatch(t, matches[1], []string{
		"ticket_6",
	}, true, "", 4)
}

// There are existing backfills, so we shall cycle through them first
// We expect to receive two different matches for two existing backfills
func TestExistingBackfills(t *testing.T) {
	setup()
	backfills := map[string][]*pb.Backfill{
		poolName1: {
			makeBackfill(backfillId1, 3),
			makeBackfill(backfillId2, 2),
		},
	}
	poolTickets := map[string][]*pb.Ticket{
		poolName1: {
			makeTicket(),
			makeTicket(),
			makeTicket(),
			makeTicket(),
		},
	}
	matches, err := mmf.MakeMatches(matchProfile, backfills, poolTickets)

	assert.Nilf(t, err, "Error message was not nil")
	assert.Equal(t, 2, len(matches), "The number of matches was not equal to 2")
	checkMatch(t, matches[0], []string{
		"ticket_1", "ticket_2",
	}, false, backfillId2, 0)
	checkMatch(t, matches[1], []string{
		"ticket_3", "ticket_4",
	}, false, backfillId1, 1)
}

// There is one backfill, but it's not enough to satisfy everybody.
// We expect to fill the existing backfill and create a new one
func TestMixedBackfills(t *testing.T) {
	setup()
	backfills := map[string][]*pb.Backfill{
		poolName1: {
			makeBackfill(backfillId1, 1),
		},
	}
	poolTickets := map[string][]*pb.Ticket{
		poolName1: {
			makeTicket(),
			makeTicket(),
			makeTicket(),
		},
	}
	matches, err := mmf.MakeMatches(matchProfile, backfills, poolTickets)

	assert.Nilf(t, err, "Error message was not nil")
	assert.Equal(t, 2, len(matches), "The number of matches was not equal to 2")
	checkMatch(t, matches[0], []string{
		"ticket_1",
	}, false, backfillId1, 0)
	checkMatch(t, matches[1], []string{
		"ticket_2", "ticket_3",
	}, true, "", 3)
}

/************************************************************************************************/

func checkMatch(
	t *testing.T, actualMatch *pb.Match, expectedTicketIds []string,
	isAllocateNew bool, backfillId string, freeSlots int32,
) {
	assert.Equal(t, matchProfileName, actualMatch.MatchProfile, "Wrong match profile name")
	assert.Equal(t, mmf.MatchFunctionName, actualMatch.MatchFunction, "Wrong match function name")
	assert.Equal(t, isAllocateNew, actualMatch.AllocateGameserver, "Wrong AllocateGameserver value")

	var freeSlotsWrapper wrapperspb.Int32Value
	err := anypb.UnmarshalTo(actualMatch.Backfill.Extensions[mmf.FreeSlotsLabel], &freeSlotsWrapper, proto.UnmarshalOptions{})
	if err != nil {
		panic(err)
	}
	assert.Equal(t, freeSlots, freeSlotsWrapper.Value, "Wrong number of free slots")
	if !isAllocateNew {
		assert.Equal(t, backfillId, actualMatch.Backfill.Id, "Wrong backfill id")
	}
	for ticket_index, ticket := range actualMatch.GetTickets() {
		assert.Equal(t, expectedTicketIds[ticket_index], ticket.Id, "Wrong ticket id")
	}
}

func makeTicket() *pb.Ticket {
	currentPlayerId++
	return &pb.Ticket{Id: fmt.Sprintf("ticket_%d", currentPlayerId), Extensions: map[string]*anypb.Any{}}
}

func makeBackfill(backfillId string, numberOfSlots int32) *pb.Backfill {
	backfill := &pb.Backfill{
		Id:         backfillId,
		Extensions: map[string]*anypb.Any{},
	}
	mmf.SetBackfillFreeSlots(backfill, numberOfSlots)
	return backfill
}

func makeMatchProfile() *pb.MatchProfile {
	mp := &pb.MatchProfile{Name: matchProfileName}
	extensions := make(map[string]*anypb.Any)
	playerCount, err := anypb.New(wrapperspb.Int32(numberOfPlayers))
	if err != nil {
		panic(err)
	}
	extensions["number_of_players"] = playerCount
	mp.Extensions = extensions
	return mp
}

func setup() {
	currentPlayerId = 0
}
