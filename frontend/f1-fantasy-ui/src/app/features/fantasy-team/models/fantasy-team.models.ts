export interface CreateFantasyTeamRequest {
  name: string;
  constructorIds: number[];
  driverIds: number[];
}

export interface FantasyTeamDriver {
  id: number;
  firstName: string;
  lastName: string;
  fullName: string;
  code: string;
  price: number;
  constructorId: number;
  constructorName: string;
  constructorCode: string;
  pointsAtTransfer: number;
}

export interface FantasyTeamConstructor {
  id: number;
  name: string;
  code: string;
  price: number;
  pointsAtTransfer:number;
}

export interface FantasyTeam {
  id: number;
  name: string;
  budgetCap: number;
  remainingBudget: number;
  hasUsedTransfer: boolean;
  lockedInPoints: number;
  userId: number;
  username: string;
  constructors: FantasyTeamConstructor[];
  drivers: FantasyTeamDriver[];
}
