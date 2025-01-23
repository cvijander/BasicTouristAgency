using BasicTouristAgency.Models;

namespace BasicTouristAgency.Services
{
       
    public class UserService : IUserService
    {
        private readonly BasicTouristAgenctDbContext _dbContext;

        public UserService (BasicTouristAgenctDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void CreateUser(User user)
        {
            _dbContext.Users.Add(user);
        }

        public void DeleteUser(int id)
        {
            User user = _dbContext.Users.Find(id);
            if(user != null)
            {
                _dbContext.Users.Remove(user);
            }
        }

        public IEnumerable<User> GetAllUsers()
        {
            return _dbContext.Users.ToList();
        }

        public User GetUserByEmail(string email)
        {
            return _dbContext.Users.FirstOrDefault(u => u.Email == email);
        }

        public User GetUserById(int id)
        {
            return _dbContext.Users.Find(id);
        }

        public void UpdateUser(User user)
        {
            _dbContext.Users.Update(user);
        }
    }
}
