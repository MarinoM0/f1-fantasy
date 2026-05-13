export interface LeagueSummary {
    id: number;
    name:string;
    memberCount: number;
    isOwner: boolean;
    createdAtUtc: string;
}

export interface LeagueMember {
    userId: number;
    username: string;
    teamName: string | null;
    isOwner: boolean;
    joinedAtUtc: string;
}

export interface League {
    id: number;
    name:string;
    inviteCode:string;
    ownerId: number;
    ownerUsername: string;
    memberCount: number;
    isOwner: boolean;
    createdAtUtc: string;
    members: LeagueMember[];
}

export interface LeagueLeaderboardEntry {
  rank: number;
  userId: number;
  username: string;
  teamName: string;
  totalPoints: number;
}

export interface CreateLeagueRequest {
  name: string;
}

export interface JoinLeagueRequest {
  inviteCode: string;
}