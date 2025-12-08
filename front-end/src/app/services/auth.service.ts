import { Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import * as CONST from "../app-const";
import { firstValueFrom } from 'rxjs';
import { AuthentificationService, LoginRequestDtoIn, LoginResponse, UserModel, UserService,  } from 'src/app/generated';
import ModuleService from 'src/app/services/security/module.service';

@Injectable({
  providedIn: 'root',
})


export class AuthService {

  authState: any;



  constructor(private router: Router, private authentificationService: AuthentificationService, private userService: UserService) {
    document.addEventListener("mousemove", () => { }, { passive: false, capture: true })
  }
public async login(username: string, password: string): Promise<boolean> {
  const loginRequestDtoIn: LoginRequestDtoIn = {
    login: username,
    password: password,
  };

  try {
    const res = await firstValueFrom(this.authentificationService.authentificationControllerLogin(loginRequestDtoIn));

    // store token only
    this.saveToken(res);

    // now token is stored → safe to call current-user
    const user = await firstValueFrom<UserModel>(this.userService.userControllerGetCurrentUSer());
    this.saveUser(user);

    return true;
  } catch (err) {
    return false;
  }
}


  public logout(redirectUrl = "/auth/login"): void {

    this.deconsteStoredTokenAndUser();
    localStorage.clear();
    this.router.navigate([redirectUrl], {
      queryParams: { action: true },
    });
  }

  public isLogged(): boolean {

    const user = localStorage.getItem(CONST.CURRENT_USER);
    const currentUser = user ? JSON.parse(user) : null;
    const token = localStorage.getItem(CONST.TOKEN_VALUE);
    return Boolean(currentUser && token);
  }





public getUser(): UserModel | null {
  const user = localStorage.getItem(CONST.CURRENT_USER);
  if (!user) return null;

  try {
    return JSON.parse(user) as UserModel;
  } catch {
    return null;
  }
}




  public setUser(user: UserModel): void {
    localStorage.setItem(CONST.CURRENT_USER, JSON.stringify(user));
  }

  public getAccessToken() {
    return localStorage.getItem(CONST.TOKEN_VALUE);
  }

  private async storeTokenAndLoadUser(token: LoginResponse) {
  this.saveToken(token);
  return true; // remove current-user call
}


  public getTokenExpireDate(): Date | null {
    const exp = localStorage.getItem(CONST.TOKEN_EXPIRES_IN);
    return exp ? new Date(JSON.parse(exp)) : null;
  }

  private saveToken(token: LoginResponse) {
    if (token) {
      const expireDate = new Date(token.expierationDate!);
      localStorage.setItem(CONST.TOKEN_VALUE, token.token!);
      localStorage.setItem(CONST.TOKEN_EXPIRES_IN, JSON.stringify(expireDate));
    }
  }

  private saveUser(user: UserModel) {
    localStorage.setItem(CONST.CURRENT_USER, JSON.stringify(user));
  }



  deleteBuisnessDay() {
    localStorage.removeItem(CONST.CURRENT_BUSINESS_DAY);
  }

  deconsteStoredTokenAndUser(): void {
    localStorage.removeItem(CONST.CURRENT_USER);
    localStorage.removeItem(CONST.TOKEN_VALUE);
    localStorage.removeItem(CONST.TOKEN_EXPIRES_IN);
  }

  async getCurrentUser(){
    const user = await firstValueFrom(this.userService.userControllerGetCurrentUSer())
    return user!=null;
  }

}
