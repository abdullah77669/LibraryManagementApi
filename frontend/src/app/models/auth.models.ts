export interface LoginRequest {
  userID: string;
  password: string;
}

export interface RegisterRequest {
  userID: string;
  fullName: string;
  email: string;
  phoneNumber: string;
  address: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  expiresAt: string;
  userID: string;
  role: string;
}
