﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Warewolf.Interfaces;
using Warewolf.UnitTestAttributes;

namespace Warewolf.Driver.Redis.Tests
{
     [TestFixture]
     public class RedisCacheTests
     {
         [Test]
         [Category("RedisCache")]
         [Author("Hagashen Naidu")]
         [ExpectedException(typeof(ArgumentNullException))]
         public void RedisCache_Constructor_NullFunction_ShouldThrowException()
         {
             //--------------Arrange------------------------------
             //--------------Act----------------------------------
             new RedisCacheStub(null);
             //--------------Assert-------------------------------
         }

         [Test]
         [Category("RedisCache")]
         [Author("Candice Daniel")]
         [ExpectedException(typeof(ArgumentNullException))]
         public void RedisCache_Decrement_NullKey_ShouldThrowArgumentNullException()
         {
             //--------------Arrange------------------------------
             var ext = TaskExtensions.Unwrap(new Task<Task>(() => Task.FromResult(true)));
             var mockConnection = new Mock<IRedisConnection>();
             var mockDatabase = new Mock<IRedisCache>();
             mockDatabase.Setup(db => db.Decrement(It.IsAny<string>(), "1")).Verifiable();
             mockConnection.Setup(conn => conn.Cache).Returns(mockDatabase.Object);
             var redis = new RedisCacheStub(() => mockConnection.Object);
             //--------------Act----------------------------------
             redis.Decrement(null, "1");
             //--------------Assert-------------------------------
         }

         [Test]
         [Category("RedisCache")]
         [Author("Candice Daniel")]
         public void RedisCache_Decrement_ValidKey_ShouldDecrement()
         {
             //--------------Arrange------------------------------
             var mockConnection = new Mock<IRedisConnection>();
             var mockDatabase = new Mock<IRedisCache>();
             mockDatabase.Setup(db => db.Decrement("bob", "1")).Verifiable();
             mockConnection.Setup(conn => conn.Cache).Returns(mockDatabase.Object);
             var redis = new RedisCacheStub(() => mockConnection.Object);
             //--------------Act----------------------------------
             redis.Decrement("bob", "1");
             //--------------Assert-------------------------------
             mockDatabase.Verify(db => db.Decrement("bob", "1"), Times.Once);
         }

         [Test]
         [Category("RedisCache")]
         [Author("Candice Daniel")]
         [ExpectedException(typeof(ArgumentNullException))]
         public void RedisCache_Delete_NullKey_ShouldThrowArgumentNullException()
         {
             //--------------Arrange------------------------------
             var ext = TaskExtensions.Unwrap(new Task<Task>(() => Task.FromResult(true)));
             var mockConnection = new Mock<IRedisConnection>();
             var mockDatabase = new Mock<IRedisCache>();
             mockDatabase.Setup(db => db.Remove(It.IsAny<string>())).Verifiable();
             mockConnection.Setup(conn => conn.Cache).Returns(mockDatabase.Object);
             var redis = new RedisCacheStub(() => mockConnection.Object);
             //--------------Act----------------------------------
             redis.Remove(null);
             //--------------Assert-------------------------------
         }

         [Test]
         [Category("RedisCache")]
         [Author("Candice Daniel")]
         public void RedisCache_Delete_ValidKey_ShouldReturn()
         {
             //--------------Arrange------------------------------
             var mockConnection = new Mock<IRedisConnection>();
             var mockDatabase = new Mock<IRedisCache>();
             mockDatabase.Setup(db => db.Remove("bob")).Verifiable();
             mockConnection.Setup(conn => conn.Cache).Returns(mockDatabase.Object);
             var redis = new RedisCacheStub(() => mockConnection.Object);
             //--------------Act----------------------------------
             redis.Remove("bob");
             //--------------Assert-------------------------------
             mockDatabase.Verify(db => db.Remove("bob"), Times.Once);
         }


         [Test]
         [Category("RedisCache")]
         [Author("Hagashen Naidu")]
         [ExpectedException(typeof(ArgumentNullException))]
         public void RedisCache_Get_NullKey_ShouldThrowArgumentNullException()
         {
             //--------------Arrange------------------------------
             var ext = TaskExtensions.Unwrap(new Task<Task>(() => Task.FromResult(true)));
             var mockConnection = new Mock<IRedisConnection>();
             var mockDatabase = new Mock<IRedisCache>();
             mockDatabase.Setup(db => db.Get(It.IsAny<string>())).Verifiable();
             mockConnection.Setup(conn => conn.Cache).Returns(mockDatabase.Object);
             var redis = new RedisCacheStub(() => mockConnection.Object);
             //--------------Act----------------------------------
             redis.Get(null);
             //--------------Assert-------------------------------
         }

         [Test]
         [Category("RedisCache")]
         [Author("Hagashen Naidu")]
         public void RedisCache_Get_ValidKey_ShouldReturn()
         {
             //--------------Arrange------------------------------
             var mockConnection = new Mock<IRedisConnection>();
             var mockDatabase = new Mock<IRedisCache>();
             mockDatabase.Setup(db => db.Get("bob")).Verifiable();
             mockConnection.Setup(conn => conn.Cache).Returns(mockDatabase.Object);
             var redis = new RedisCacheStub(() => mockConnection.Object);
             //--------------Act----------------------------------
             redis.Get("bob");
             //--------------Assert-------------------------------
             mockDatabase.Verify(db => db.Get("bob"), Times.Once);
         }

         [Test]
         [Category("RedisCache")]
         [Author("Candice Daniel")]
         [ExpectedException(typeof(ArgumentNullException))]
         public void RedisCache_Increment_NullKey_ShouldThrowArgumentNullException()
         {
             //--------------Arrange------------------------------
             var ext = TaskExtensions.Unwrap(new Task<Task>(() => Task.FromResult(true)));
             var mockConnection = new Mock<IRedisConnection>();
             var mockDatabase = new Mock<IRedisCache>();
             mockDatabase.Setup(db => db.Increment(It.IsAny<string>(), "1")).Verifiable();
             mockConnection.Setup(conn => conn.Cache).Returns(mockDatabase.Object);
             var redis = new RedisCacheStub(() => mockConnection.Object);
             //--------------Act----------------------------------
             redis.Increment(null, "1");
             //--------------Assert-------------------------------
         }

         [Test]
         [Category("RedisCache")]
         [Author("Candice Daniel")]
         public void RedisCache_Increment_ValidKey_ShouldIncrement()
         {
             //--------------Arrange------------------------------
             var mockConnection = new Mock<IRedisConnection>();
             var mockDatabase = new Mock<IRedisCache>();
             mockDatabase.Setup(db => db.Increment("bob", "1")).Verifiable();
             mockConnection.Setup(conn => conn.Cache).Returns(mockDatabase.Object);
             var redis = new RedisCacheStub(() => mockConnection.Object);
             //--------------Act----------------------------------
             redis.Increment("bob", "1");
             //--------------Assert-------------------------------
             mockDatabase.Verify(db => db.Increment("bob", "1"), Times.Once);
         }

         [Test]
         [Author("Siphamandla Dube")]
         [Category("RedisCache")]
         public void RedisCache_Set_DataToCacheWithNoTTL_CachedDataShouldNotExpire()
         {
             //----------------------Arrange----------------------
             var mockConnection = new Mock<IRedisConnection>();
             var mockDatabase = new Mock<IRedisCache>();

             mockDatabase.Setup(db => db.Set(It.IsAny<string>(), It.IsAny<string>(), TimeSpan.FromMilliseconds(0)))
                 .Returns(true);
             mockConnection.Setup(conn => conn.Cache).Returns(mockDatabase.Object);

             var sut = new RedisCacheStub(() => mockConnection.Object);
             //----------------------Act--------------------------
             var result = sut.Set("key1", "data to store in cache", TimeSpan.FromMilliseconds(0));
             //----------------------Assert-----------------------
             Assert.IsTrue(result);
         }

         [Test]
         [Category("RedisCache")]
         [Author("Hagashen Naidu")]
         [ExpectedException(typeof(ArgumentNullException))]
         public void RedisCache_Set_NotStringValue_ShouldThrowInvalidOperationException()
         {
             //--------------Arrange------------------------------
             var mockConnection = new Mock<IRedisConnection>();
             var mockDatabase = new Mock<IRedisCache>();
             mockDatabase.Setup(db => db.Set(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>())).Verifiable();
             mockConnection.Setup(conn => conn.Cache).Returns(mockDatabase.Object);
             var redis = new RedisCacheStub(() => mockConnection.Object);
             //--------------Act----------------------------------
             redis.Set("bob", It.IsAny<string>(), It.IsAny<TimeSpan>());
             //--------------Assert-------------------------------
         }

         [Test]
         [Category("RedisCache")]
         [Author("Hagashen Naidu")]
         [ExpectedException(typeof(ArgumentNullException))]
         public void RedisCache_Set_NullKey_ShouldThrowArgumentNullException()
         {
             //--------------Arrange------------------------------
             var mockConnection = new Mock<IRedisConnection>();
             var mockDatabase = new Mock<IRedisCache>();
             mockDatabase.Setup(db => db.Set(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>())).Verifiable();
             mockConnection.Setup(conn => conn.Cache).Returns(mockDatabase.Object);
             var redis = new RedisCacheStub(() => mockConnection.Object);
             //--------------Act----------------------------------
             redis.Set(null, string.Empty, It.IsAny<TimeSpan>());
             //--------------Assert-------------------------------
         }

         [Test]
         [Category("RedisCache")]
         [Author("Hagashen Naidu")]
         [ExpectedException(typeof(ArgumentNullException))]
         public void RedisCache_Set_NullValue_ShouldThrowArgumentNullException()
         {
             //--------------Arrange------------------------------
             var mockConnection = new Mock<IRedisConnection>();
             var mockDatabase = new Mock<IRedisCache>();
             mockDatabase.Setup(db => db.Set(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>())).Verifiable();
             mockConnection.Setup(conn => conn.Cache).Returns(mockDatabase.Object);
             var redis = new RedisCacheStub(() => mockConnection.Object);
             //--------------Act----------------------------------
             redis.Set("bob", null, It.IsAny<TimeSpan>());
             //--------------Assert-------------------------------
         }

         [Test]
         [Category("RedisCache")]
         [Author("Hagashen Naidu")]
         public void RedisCache_Set_StringValue_ShouldCallStringSetOnCache()
         {
             //--------------Arrange------------------------------
             var mockConnection = new Mock<IRedisConnection>();
             var mockDatabase = new Mock<IRedisCache>();
             mockDatabase.Setup(db => db.Set(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>())).Verifiable();
             mockConnection.Setup(conn => conn.Cache).Returns(mockDatabase.Object);
             var redis = new RedisCacheStub(() => mockConnection.Object);
             //--------------Act----------------------------------
             redis.Set("bob", "the builder", It.IsAny<TimeSpan>());
             //--------------Assert-------------------------------
             mockDatabase.Verify(db => db.Set(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()), Times.Once);
         }
     }

     internal class RedisCacheStub : RedisCacheBase
     {
         public RedisCacheStub(Func<IRedisConnection> createConnection) : base(createConnection)
         {
         }
     }
}