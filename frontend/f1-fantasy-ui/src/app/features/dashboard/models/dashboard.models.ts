export interface Dashboard {
    upcomingRace: DashboardUpcomingRace | null;
    leaderboard: DashboardLeaderboardEntry[];
}

export interface DashboardUpcomingRace {
    localRaceId: number | null;
    season: string;
    roundNumber: number;
    raceName: string;
    circuitId: string;
    circuitName: string;
    country: string;
    locality: string;
    startTimeUtc: string | null;
    dataSource: 'jolpica' | 'local' | string;
}

export interface DashboardLeaderboardEntry {
    rank:number;
    userId: number;
    username: string;
    teamName: string;
    totalPoints: number;
}

export interface DashboardCountdown {
    days:number;
    hours:number;
    minutes: number;
    seconds: number;
    isStarted: boolean;
}