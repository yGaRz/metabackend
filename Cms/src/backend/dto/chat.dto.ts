export interface ChatMessageResponse {
    messageId: number;
    senderName: string;
    senderUserId: string;
    text: string;
    channelId: string;
    channelType: string;
    created: Date;
}
