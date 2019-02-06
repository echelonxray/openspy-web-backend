﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CoreWeb.Controllers.generic;
using CoreWeb.Models;
using CoreWeb.Repository;
using Microsoft.AspNetCore.Authorization;
using CoreWeb.Exception;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CoreWeb.Controllers
{
    [Authorize(Policy = "ProfileManage")]
    public class ProfileController : ModelController<Profile, ProfileLookup>
    {
        private IRepository<User, UserLookup> userRepository;
        private ProfileRepository profileRepository;
        public ProfileController(IRepository<Profile, ProfileLookup> repository, IRepository<User, UserLookup> userRepository) : base(repository)
        {
            this.userRepository = userRepository;
            this.profileRepository = (ProfileRepository)repository;
        }
        // POST api/<controller>
        [HttpPost]
        public override async Task<Profile> Update([FromBody]Profile value)
        {
            //namespaceid 0 cannot have unique nicks
            if (value.Namespaceid == 0 && value.Uniquenick.Length != 0)
            {
                value.Uniquenick = "";
            }
            if (await PerformUniqueNickChecks(value))
            {
                return await base.Update(value);
            }
            return null;            
        }

        // PUT api/<controller>/5
        [HttpPut]
        public override async Task<Profile> Put([FromBody]Profile value)
        {
            //namespaceid 0 cannot have unique nicks
            if (value.Namespaceid == 0 && value.Uniquenick.Length != 0)
            {
                value.Uniquenick = "";
            }
            if (await PerformUniqueNickChecks(value) && await PerformNickChecks(value))
            {
                return await base.Put(value);
            }
            return null;
                
        }
        [HttpDelete]
        public override async Task<DeleteStatus> Delete([FromBody] ProfileLookup lookup)
        {
            var profileLookup = new ProfileLookup();
            profileLookup.user = new UserLookup();
            profileLookup.user.id = lookup.user.id;

            var profiles = (await profileRepository.Lookup(profileLookup));
            if(profiles.ToArray().Length <= 1)
            {
                throw new CannotDeleteLastProfileException();
            }

            return await base.Delete(lookup);
        }
        //CannotDeleteLastProfileException
        private async Task<bool> PerformNickChecks(Profile value)
        {
            if (!profileRepository.CheckUniqueNickValid(value.Uniquenick, value.Namespaceid))
            {
                throw new UniqueNickInvalidException();
            }
            var profileLookup = new ProfileLookup();
            profileLookup.id = value.Id;
            profileLookup.nick = value.Nick;
            profileLookup.uniquenick = value.Uniquenick;
            profileLookup.namespaceid = value.Namespaceid;
            profileLookup.user = new UserLookup();
            profileLookup.user.id = value.Userid;

            var profile = (await profileRepository.Lookup(profileLookup)).FirstOrDefault();
            if (profile == null)
            {
                return false;
            }
            throw new NickInvalidException();
        }
        private async Task<bool> PerformUniqueNickChecks(Profile value)
        {
            if(!profileRepository.CheckUniqueNickValid(value.Uniquenick, value.Namespaceid))
            {
                throw new UniqueNickInvalidException();
            }
            var profileLookup = new ProfileLookup();
            profileLookup.id = value.Id;
            profileLookup.nick = value.Nick;
            profileLookup.uniquenick = value.Uniquenick;
            profileLookup.namespaceid = value.Namespaceid;
            profileLookup.user = new UserLookup();
            profileLookup.user.id = value.Userid;

            var profile = (await profileRepository.Lookup(profileLookup)).FirstOrDefault();
            if (profile == null)
            {
                throw new NoSuchUserException();
            }
            var userLookup = new UserLookup();
            var user = (await userRepository.Lookup(userLookup)).FirstOrDefault();
            if (user == null)
            {
                throw new NoSuchUserException();
            }
            var checkData = await profileRepository.CheckUniqueNickInUse(profile.Uniquenick, profile.Namespaceid, user.Partnercode);
            if (checkData.Item1)
            {
                if(checkData.Item2 != value.Id)
                    throw new UniqueNickInUseException(checkData.Item2, checkData.Item3);
            }
            return true;
        }
    }
}
