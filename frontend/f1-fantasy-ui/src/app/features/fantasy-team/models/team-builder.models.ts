export interface TeamBuilderDriver {
    id: number;
    firstName: string;
    lastName: string;
    fullName: string;
    code: string;
    price: number;
    constructorId: number;
    constructorName: string;
    constructorCode: string;
    standingPosition: number | null;
    currentPoints: number;
    currentWins: number;
    hasLiveData: boolean;
}

export interface TeamBuilderConstructor {
    id: number;
    name: string;
    code: string;
    price: number;
    standingPosition: number | null;
    currentPoints: number;
    currentWins: number;
    hasLiveData: boolean;
}