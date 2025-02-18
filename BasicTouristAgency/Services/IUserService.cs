using BasicTouristAgency.Models;

namespace BasicTouristAgency.Services
{
    public interface IUserService
    {
        IEnumerable<User> GetAllUsers();

        User GetUserById(string userId);

        void CreateUser (User user);

        void UpdateUser (User user);

        void DeleteUser (int id);

        User GetUserByEmail(string email);



    }
}
