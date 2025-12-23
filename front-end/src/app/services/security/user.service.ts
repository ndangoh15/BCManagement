import { ToastrService } from 'ngx-toastr';
import { Injectable } from '@angular/core';
import { Subject, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { UserCreateDTO, UserModel, UserService } from 'src/app/generated';
import { closeModal } from 'src/app/helper/helper-function';

@Injectable({
  providedIn: 'root'
})
export class UserManagerService {
  private userList: UserModel[] = [];
  userListSubject = new Subject<UserModel[]>();

  constructor(private userService: UserService, private toastrService: ToastrService) {

  }

  emitUserList(): void {
    this.userListSubject.next(this.userList.slice());
  }

  refreshUserList(): void {
    this.userService.userControllerGetUsersWithLowerAccessLevel().pipe(
      catchError(error => {
        this.userListSubject.error(error);
        return throwError(() => new Error('Failed to get users', error));
      })
    ).subscribe(users => {
      this.userList = users.filter(u => u.isConnected);
      this.emitUserList();
    });
  }

  addUser(user: UserCreateDTO) {
    return this.userService.userControllerCreateUser(user);
  }

  updateUser(user: UserCreateDTO) {
    return this.userService.userControllerUpdateUser(user);
  }

  deleteUser(id: number): void {
    this.userService.userControllerDeleteUser(id).pipe(
      catchError(error => {
        this.userListSubject.error(error);

        return throwError(() => new Error('Failed to delete user', error));
      })
    ).subscribe(() => {
      this.userList = this.userList.filter(user => user.globalPersonID !== id);

      this.toastrService.success("deleted successfuly")
      this.emitUserList();
      this.emitUserList();
    });
  }


  closePopup() {
    closeModal("user-create-form")
  }
}
