export interface Ticket {
    id: string;
    title: string;
    description: string;
    priority: 'Low' | 'Medium' | 'High' | 'Critical';
    status: 'Open' | 'InProgress' | 'Resolved' | 'Closed';
    createdAt: string;
    updatedAt?: string;
    createdBy: string;
}

export interface Comment {
    id: string;
    text: string;
    createdAt: string;
    createdBy: string;
}

export interface PaginatedResult<T> {
    totalCount: number;
    items: T[];
}

export interface CreateTicketDto {
    title: string;
    description: string;
    priority: string;
}

export interface CreateCommentDto {
    text: string;
}
