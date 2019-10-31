﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using ServiceStack.Redis;
using Warewolf.Interfaces;

namespace Warewolf.Driver.Redis
{
    public class RedisConnection : IRedisConnection
    {
        public RedisConnection(string hostName)
        {
            var client = new RedisClient(hostName);
            Cache = new RedisCache(client);
        }
        public RedisConnection(string hostName, int port)
        {
            var client = new RedisClient(hostName, port);
            Cache = new RedisCache(client);
        }
        public RedisConnection(string hostName, int port, string password)
        {
            var client = new RedisClient(hostName, port, password);
            Cache = new RedisCache(client);
        }
        public RedisConnection(System.Uri uri)
        {
            var client = new RedisClient(uri);
            Cache = new RedisCache(client);
        }
        public IRedisCache Cache { get; private set; }
    }

    internal class RedisCache : IRedisCache
    {
        private readonly RedisClient _client;

        public RedisCache(RedisClient client)
        {
            _client = client;
        }

        public bool HashSet(string key, IDictionary<string, string> dictionary)
        {
            _client.SetRangeInHash(key, dictionary);
            return true;
        }

        public string Get(string key) => _client.Get<string>(key);
        public bool Set(string key, string value) => _client.Set(key, value);
    }
}