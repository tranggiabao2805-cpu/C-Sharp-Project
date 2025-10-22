﻿using System;
namespace FastFoodOnline.Services
{
    public interface IPasswordHasher
    {
        (string Hash, string Salt) HashPassword(string password);
        bool Verify(string password, string hash, string salt);
    }
}
