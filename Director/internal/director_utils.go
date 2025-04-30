package director

import (
	"fmt"
	"encoding/json"
	"log"
	"errors"

	"google.golang.org/protobuf/proto"
	"google.golang.org/protobuf/types/known/anypb"
	"google.golang.org/protobuf/types/known/wrapperspb"
	"open-match.dev/open-match/pkg/pb"
)

const (
	FreeSlotsLabel       = "open-slots"
	NumberOfPlayersLabel = "numberOfPlayers"
    GameServerUrlLabel   = "GameServerUrl"
)

func GetFreeSlotsFromBackfill(backfill *pb.Backfill) (int32, error) {
    if backfill == nil {
        return 0, errors.New("backfill does not exist")
    }
    extensions := backfill.Extensions
    if extensions == nil {
        return 0, errors.New("extensions do not exist")
    }
    anyVal, ok := extensions[FreeSlotsLabel]
    if !ok {
        return 0, errors.New("open-slots not found in extensions")
    }
    var freeSlots wrapperspb.Int32Value
    err := anypb.UnmarshalTo(anyVal, &freeSlots, proto.UnmarshalOptions{})
    if err != nil {
        return 0, err
    }
    return freeSlots.Value, nil
}

func getGameServerUrlFromBackfill(backfill *pb.Backfill) (string, error) {
    if backfill.Extensions == nil {
        return "", fmt.Errorf("extensions do not exist")
    }

    if any, ok := backfill.Extensions[GameServerUrlLabel]; ok {
        var val wrapperspb.StringValue
        err := any.UnmarshalTo(&val)
        if err != nil {
            return "", err
        }
        return val.Value, nil
    }

    return "", fmt.Errorf("GameServerUrl not found in extensions")
}

func setGameServerUrlInBackfill(backfill *pb.Backfill, url string) error {
    if backfill.Extensions == nil {
        backfill.Extensions = make(map[string]*anypb.Any)
    }

    any, err := anypb.New(&wrapperspb.StringValue{Value: url})
    if err != nil {
        return err
    }

    backfill.Extensions[GameServerUrlLabel] = any
    return nil
}

func logMatch(match *pb.Match) {
    // Сериализуем предложение в JSON
    matchJSON, err := json.MarshalIndent(match, "", "  ")
    if err != nil {
        log.Printf("Error serializing proposal to JSON: %v", err)
        return
    }

    // Логируем JSON-представление предложения
    log.Printf("Proposal: %s", string(matchJSON))
}

func setFreeSlotsInBackfill(backfill *pb.Backfill, val int32) error {
    if backfill.Extensions == nil {
        backfill.Extensions = make(map[string]*anypb.Any)
    }

    anyVal, err := anypb.New(&wrapperspb.Int32Value{Value: val})
    if err != nil {
        return err
    }

    backfill.Extensions[FreeSlotsLabel] = anyVal
    return nil
}
