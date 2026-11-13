using System.Collections.ObjectModel;
using Rxbit.Brigade.Controll.Mvvm;

namespace Rxbit.Brigade.Controll.ViewModels
{
    public class AdminUserItem : ObservableObject
    {
        private int _id;
        private string _loginName;
        private string _phone;
        private string _email;
        private string _role;
        private string _joinTime;
        private string _status;

        public int Id { get => _id; set { SetProperty(ref _id, value); } }
        public string LoginName { get => _loginName; set { SetProperty(ref _loginName, value); } }
        public string Phone { get => _phone; set { SetProperty(ref _phone, value); } }
        public string Email { get => _email; set { SetProperty(ref _email, value); } }
        public string Role { get => _role; set { SetProperty(ref _role, value); } }
        public string JoinTime { get => _joinTime; set { SetProperty(ref _joinTime, value); } }
        public string Status { get => _status; set { SetProperty(ref _status, value); } }
    }

    public class AdminListViewModel : ObservableObject
    {
        private ObservableCollection<AdminUserItem> _users;
        private string _searchDateStart;
        private string _searchDateEnd;
        private string _searchUserName;
        private int _currentPage;
        private int _totalPages;
        private int _totalCount;

        public ObservableCollection<AdminUserItem> Users { get => _users; set { SetProperty(ref _users, value); } }
        public string SearchDateStart { get => _searchDateStart; set { SetProperty(ref _searchDateStart, value); } }
        public string SearchDateEnd { get => _searchDateEnd; set { SetProperty(ref _searchDateEnd, value); } }
        public string SearchUserName { get => _searchUserName; set { SetProperty(ref _searchUserName, value); } }
        public int CurrentPage { get => _currentPage; set { SetProperty(ref _currentPage, value); } }
        public int TotalPages { get => _totalPages; set { SetProperty(ref _totalPages, value); } }
        public int TotalCount { get => _totalCount; set { SetProperty(ref _totalCount, value); } }

        public AdminListViewModel()
        {
            SearchDateStart = "";
            SearchDateEnd = "";
            SearchUserName = "";
            CurrentPage = 1;
            TotalPages = 489;
            TotalCount = 4890;

            Users = new ObservableCollection<AdminUserItem>
            {
                new AdminUserItem { Id = 1, LoginName = "admin", Phone = "1892519194", Email = "11366400@qq.com", Role = "超级管理员", JoinTime = "2017-01-01 11:42", Status = "已启用" },
                new AdminUserItem { Id = 2, LoginName = "editor01", Phone = "18900123456", Email = "editor@test.com", Role = "编辑", JoinTime = "2017-03-15 09:30", Status = "已启用" },
                new AdminUserItem { Id = 3, LoginName = "operator", Phone = "18911223344", Email = "op@test.com", Role = "运营", JoinTime = "2017-05-20 14:22", Status = "已禁用" },
                new AdminUserItem { Id = 4, LoginName = "finance", Phone = "18933445566", Email = "finance@test.com", Role = "财务", JoinTime = "2017-07-08 16:45", Status = "已启用" },
                new AdminUserItem { Id = 5, LoginName = "manager01", Phone = "18977889900", Email = "mgr@test.com", Role = "管理员", JoinTime = "2017-09-12 10:15", Status = "已启用" },
            };
        }
    }
}
