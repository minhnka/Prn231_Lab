using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class RoleDAO
    {
        private static RoleDAO instance;
        private static ProductManagementDbContext dbContext;

        public RoleDAO()
        {
            dbContext = new ProductManagementDbContext();
        }
        public static RoleDAO Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new RoleDAO();
                }
                return instance;
            }
        }


        // READ ALL
        public List<Role> GetAll()
        {
            return dbContext.Roles.ToList();
        }

        // READ BY ID
        public Role GetById(int id)
        {
            return dbContext.Roles.FirstOrDefault(h => h.RoleId == id);
        }

        // DELETE
        public void Delete(int id)
        {
            var role = dbContext.Roles.FirstOrDefault(h => h.RoleId == id);
            if (role != null)
            {
                dbContext.Roles.Remove(role);
                dbContext.SaveChanges();
            }
        }
    }
}
