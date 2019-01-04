﻿using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreWeb.Database;
using CoreWeb.Models;

namespace CoreWeb.Repository
{
    public class UserRepository : IRepository<User, UserLookup>
    {
        private GameTrackerDBContext gameTrackerDb;
        public UserRepository(GameTrackerDBContext gameTrackerDb)
        {
            this.gameTrackerDb = gameTrackerDb;
        }
        public async Task<IEnumerable<User>> Lookup(UserLookup lookup)
        {
            var query = gameTrackerDb.User as IQueryable<User>;
            if (lookup.id.HasValue)
            {
                query = query.Where(b => b.Id == lookup.id.Value);
            }
            if (lookup.email != null)
            {
                query = query.Where(b => b.Email == lookup.email);
            }
            if(lookup.partnercode.HasValue)
            {
                query = query.Where(b => b.Partnercode == lookup.partnercode.Value);
            }
            return await query.ToListAsync();
        }
        public Task<bool> Delete(UserLookup lookup)
        {
            return Task.Run(async () =>
            {
                var users = (await Lookup(lookup)).ToList();
                foreach (var user in users)
                {
                    gameTrackerDb.Remove<User>(user);
                }
                var num_modified = await gameTrackerDb.SaveChangesAsync();
                return users.Count > 0 && num_modified > 0;
            });
        }
        public Task<User> Update(User model)
        {
            return Task.Run(async () =>
            {
                var entry = gameTrackerDb.Update<User>(model);
                await gameTrackerDb.SaveChangesAsync();
                return entry.Entity;
            });
        }
        public async Task<User> Create(User model)
        {
            var entry = await gameTrackerDb.AddAsync<User>(model);
            var num_modified = await gameTrackerDb.SaveChangesAsync(true);
            return entry.Entity;
        }
    }
}
