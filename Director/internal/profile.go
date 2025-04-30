package director

import (
	"fmt"

	"google.golang.org/protobuf/types/known/anypb"
	"google.golang.org/protobuf/types/known/wrapperspb"
	"open-match.dev/open-match/pkg/pb"
)

var fleets = map[string]int32{"Capital": 10, "Mine": 10}

func GenerateProfiles() ([]*pb.MatchProfile, error) {
	var profiles []*pb.MatchProfile
	for fleet, numberOfPlayers := range fleets {
		extensions := make(map[string]*anypb.Any)

		numberOfPlayersField, err := anypb.New(wrapperspb.Int32(numberOfPlayers))
		if err != nil {
			return nil, fmt.Errorf("failed to generate match profiles because of incorrect config for fleet %s, got error %w", fleet, err)
		}
		extensions[NumberOfPlayersLabel] = numberOfPlayersField

		profiles = append(profiles, &pb.MatchProfile{
			Name: "match_profile_" + fleet,
			Pools: []*pb.Pool{
				{
					Name: "pool_" + fleet,
					TagPresentFilters: []*pb.TagPresentFilter{
						{
							Tag: fleet,
						},
					},
				},
			},
			Extensions: extensions,
		})
	}
	return profiles, nil
}
