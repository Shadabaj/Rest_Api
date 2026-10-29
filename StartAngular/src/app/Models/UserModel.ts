// employee-login.model.ts
export interface UserModel {
  firstName: string;
  lastName: string;
  dateOfBirth: string;   // ISO date string, e.g. "1995-04-12T00:00:00"
  dateOfJoinee: string;
  passWord: string;
  roleId: number;
  userName: string;
  employeeId: number;
  userId: number;
  roles: string[];
  isActive: boolean;
  createdDate: string;
  token: string;         // short-lived access JWT
  refreshToken: string;
  expiresIn: number;
}

export class UserModel implements UserModel {
  firstName = '';
  lastName = '';
  dateOfBirth = '';
  dateOfJoinee = '';
  passWord = '';
  roleId = 0;
  userName = '';
  employeeId = 0;
  userId = 0;
  roles: string[] = [];
  isActive = false;
  createdDate = '';
  token = '';
  refreshToken = '';
  expiresIn = 0;

//   constructor(init?: Partial<EmployeeLogin>) {
//     Object.assign(this, init);
//   }
}