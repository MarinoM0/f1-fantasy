export interface PredictionDriver {
  id: number;
  code: string; 
  name: string;   
}

export interface PredictionRace {
  id: number;
  roundNumber: number;
  name: string;
  country: string;
  startTimeUtc: string;
  isLocked: boolean;
  isCompleted: boolean;
}

export interface Prediction {
  id: number;
  race: PredictionRace;
  predictedP1: PredictionDriver;
  predictedP2: PredictionDriver;
  predictedP3: PredictionDriver;
  isScored: boolean;
  score: number | null;
  actualP1: PredictionDriver | null;
  actualP2: PredictionDriver | null;
  actualP3: PredictionDriver | null;
}

export interface UpcomingPrediction {
  race: PredictionRace | null;
  existingPrediction: Prediction | null;
  availableDrivers: PredictionDriver[];
}

export interface PredictionLeaderboardEntry {
  rank: number;
  userId: number;
  username: string;
  totalPoints: number;
  predictionsScored: number;
}

export interface CreatePredictionRequest {
  raceId: number;
  p1DriverId: number;
  p2DriverId: number;
  p3DriverId: number;
}