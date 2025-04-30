export interface StreamEntity {
    streamId: string;
    url: string;
    startTime: Date;
    endTime?: string | null;
    created: Date;
}

export interface StreamPaginatedResponse {
    total: number;
    data: StreamEntity[];
}
