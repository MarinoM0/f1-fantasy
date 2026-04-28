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
}

export interface FantasyTeamConstructor {
  id: number;
  name: string;
  code: string;
  price: number;
}

export interface FantasyTeam {
  id: number;
  name: string;
  budgetCap: number;
  remainingBudget: number;
  userId: number;
  username: string;
  constructors: FantasyTeamConstructor[];
  drivers: FantasyTeamDriver[];
}
