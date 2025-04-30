using NeoDaoBackend.Models.graphQL.Enums;

namespace NeoDaoBackend.Models.db;

public enum SessionStatus {
    CREATED,
    CONFIRMED,
    REMOVED
}

public static class SessionStatusMethods {
    public static SessionStatus FromGraphQL(UserSessionStatus graphQLStatus) {
        switch (graphQLStatus) {
            case UserSessionStatus.CREATED:
                return SessionStatus.CREATED;
            case UserSessionStatus.CONFIRMED:
                return SessionStatus.CONFIRMED;
            case UserSessionStatus.FAILED:
                return SessionStatus.REMOVED;
            default:
                throw new ApplicationException("Unknown user session status: " + graphQLStatus);
        }
    }
}
