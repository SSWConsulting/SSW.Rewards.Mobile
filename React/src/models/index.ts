
export interface User {
    rank: number,
    userId: number,
    name: string,
    profilePic: string,
    points: number
}

export interface DecodedJWT {
  iss: string;
  exp: number;
  nbf: number;
  aud: string;
  idp: string;
  given_name: string;
  family_name: string;
  sub: string;
  emails: string[];
  tfp: string;
  nonce: string;
  role: string;
  scp: string;
  azp: string;
  ver: string;
  iat: number;
}