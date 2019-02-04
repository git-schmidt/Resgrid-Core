﻿using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNet.Identity.EntityFramework6;
using Resgrid.Model.Repositories;
using Resgrid.Model;

namespace Resgrid.Repositories.DataRepository
{
	public class IdentityRepository: IIdentityRepository
	{
		public string connectionString =
			ConfigurationManager.ConnectionStrings.Cast<ConnectionStringSettings>()
				.FirstOrDefault(x => x.Name == "ResgridContext")
				.ConnectionString;

		public List<IdentityUser> GetAll()
		{
			using (IDbConnection db = new SqlConnection(connectionString))
			{
				return db.Query<IdentityUser>($"SELECT * FROM AspNetUsers").ToList();
			}
		}

		public IdentityUser GetUserById(string userId)
		{
			using (IDbConnection db = new SqlConnection(connectionString))
			{
				return db.Query<IdentityUser>($"SELECT * FROM AspNetUsers WHERE Id = @userId", new { userId = userId }).FirstOrDefault();
			}
		}
		//		return db.Query<IdentityUser>($"SELECT * FROM AspNetUsers WHERE Id = '@userId'", ).FirstOrDefault();
		public IdentityUser GetUserByUserName(string userName)
		{
			using (IDbConnection db = new SqlConnection(connectionString))
			{
				return db.Query<IdentityUser>($"SELECT * FROM AspNetUsers WHERE UserName = @userName", new { userName = userName }).FirstOrDefault();
			}
		}

		public async Task<IdentityUser> GetUserByUserNameAsync(string userName)
		{
			using (IDbConnection db = new SqlConnection(connectionString))
			{
				var result = await db.QueryAsync<IdentityUser>($"SELECT * FROM AspNetUsers WHERE UserName = @userName", new {userName = userName});

				return result.FirstOrDefault();
			}
		}

		public IdentityUser GetUserByEmail(string email)
		{
			using (IDbConnection db = new SqlConnection(connectionString))
			{
				return db.Query<IdentityUser>($"SELECT * FROM AspNetUsers WHERE Email = @email", new { email = email }).FirstOrDefault();
			}
		}

		public void UpdateUsername(string oldUsername, string newUsername)
		{
			using (IDbConnection db = new SqlConnection(connectionString))
			{
				db.Execute($"UPDATE [AspNetUsers] SET [UserName] = @newUsername, [NormalizedUserName] = @newUsernameUpper WHERE UserName = @oldUsername", new { newUsername = newUsername, newUsernameUpper = newUsername.ToUpper(), oldUsername = oldUsername });
			}
		}

		public void UpdateEmail(string userId, string newEmail)
		{
			using (IDbConnection db = new SqlConnection(connectionString))
			{
				db.Execute($"UPDATE [AspNetUsers] SET [Email] = @newEmail WHERE Id = @userId", new { userId = userId, newEmail = newEmail });
			}
		}

		public void AddUserToRole(string userId, string roleId)
		{
			using (IDbConnection db = new SqlConnection(connectionString))
			{
				db.Execute($"INSERT INTO AspNetUserRoles ([UserId] ,[RoleId]) VALUES (@userId, @roleId)", new { userId = userId, roleId = roleId });
			}
		}

		public void InitUserExtInfo(string userId)
		{
			using (IDbConnection db = new SqlConnection(connectionString))
			{
				db.Query<IdentityUser>($"INSERT INTO AspNetUsersExt ([UserId] ,[CreateDate] ,[LastActivityDate]) VALUES (@userId,@dateTimeNow,@dateTimeNow)", new { userId = userId, dateTimeNow = DateTime.UtcNow });
				db.Execute($"UPDATE [dbo].[AspNetUsers] SET [EmailConfirmed] = 1 WHERE Id = @userId", new { userId = userId });
			}
		}

		public IdentityUserRole GetRoleForUserRole(string userId, string roleId)
		{
			using (IDbConnection db = new SqlConnection(connectionString))
			{
				return db.Query<IdentityUserRole>($"SELECT * FROM AspNetUserRoles WHERE UserId = @userId AND RoleId = @roleId", new { userId = userId, roleId = roleId }).FirstOrDefault();
			}
		}

		public IdentityUser Update(IdentityUser user)
		{
			using (IDbConnection db = new SqlConnection(connectionString))
			{
				db.Update(user);
			}

			return user;
		}

		public List<IdentityUser> GetAllMembershipsForDepartment(int departmentId)
		{
			using (IDbConnection db = new SqlConnection(connectionString))
			{
				return db.Query<IdentityUser>(@"SELECT m.* FROM AspNetUsers m	
																						INNER JOIN DepartmentMembers dm ON dm.UserId = m.Id
																						WHERE dm.DepartmentId = @departmentId AND dm.IsDeleted = 0", new { departmentId = departmentId }).ToList();
			}
		}

		public List<IdentityUser> GetAllUsersForDepartment(int departmentId)
		{
			using (IDbConnection db = new SqlConnection(connectionString))
			{
				return db.Query<IdentityUser>(@"SELECT u.* FROM AspNetUsers u
																				INNER JOIN DepartmentMembers dm ON dm.UserId = u.Id
																				WHERE dm.DepartmentId = @departmentId AND dm.IsDeleted = 0", new { departmentId = departmentId }).ToList();
			}
		}

		public List<IdentityUser> GetAllUsersForGroup(int groupId)
		{
			using (IDbConnection db = new SqlConnection(connectionString))
			{
				return db.Query<IdentityUser>(@"SELECT u.* 
																				FROM AspNetUsers u
																				INNER JOIN DepartmentGroupMembers dgm ON u.Id = dgm.UserId
																				WHERE dgm.DepartmentGroupId = @groupId", new { groupId = groupId }).ToList();
			}
		}

		public List<IdentityUser> GetAllUsersForDepartmentWithinLimits(int departmentId, bool retrieveHidden)
		{
			using (IDbConnection db = new SqlConnection(connectionString))
			{
				return db.Query<IdentityUser>(@"DECLARE @limit INT
												IF ((SELECT COUNT(*) FROM Payments p WHERE p.DepartmentId = @departmentId) > 1)
													BEGIN
														SET @limit = (SELECT TOP 1 pl.LimitValue FROM Payments p
														INNER JOIN PlanLimits pl ON pl.PlanId = p.PlanId
														WHERE DepartmentId = @departmentId AND pl.LimitType = 1 AND p.EffectiveOn <= GETUTCDATE() AND p.EndingOn >= GETUTCDATE()
														ORDER BY PaymentId DESC)
													END
												ELSE
													BEGIN
														SET @limit = 10
													END

												SELECT TOP (@limit) u.* FROM AspNetUsers u
												INNER JOIN DepartmentMembers dm ON dm.UserId = u.Id 
												WHERE dm.DepartmentId = @departmentId AND dm.IsDeleted = 0 AND (@retrieveHidden = 1 OR (dm.IsHidden = 0 OR dm.IsHidden IS NULL)) AND (dm.IsDisabled = 0 OR dm.IsDisabled IS NULL)", 
							new { departmentId = departmentId, retrieveHidden = retrieveHidden }).ToList();
			}
		}

		public async Task<List<IdentityUser>> GetAllUsersForDepartmentWithinLimitsAsync(int departmentId, bool retrieveHidden)
		{
			using (IDbConnection db = new SqlConnection(connectionString))
			{
				var result = await db.QueryAsync<IdentityUser>(@"DECLARE @limit INT
												IF ((SELECT COUNT(*) FROM Payments p WHERE p.DepartmentId = @departmentId) > 1)
													BEGIN
														SET @limit = (SELECT TOP 1 pl.LimitValue FROM Payments p
														INNER JOIN PlanLimits pl ON pl.PlanId = p.PlanId
														WHERE DepartmentId = @departmentId AND pl.LimitType = 1 AND p.EffectiveOn <= GETUTCDATE() AND p.EndingOn >= GETUTCDATE()
														ORDER BY PaymentId DESC)
													END
												ELSE
													BEGIN
														SET @limit = 10
													END

												SELECT TOP (@limit) u.* FROM AspNetUsers u
												INNER JOIN DepartmentMembers dm ON dm.UserId = u.Id 
												WHERE dm.DepartmentId = @departmentId AND dm.IsDeleted = 0 AND (@retrieveHidden = 1 OR (dm.IsHidden = 0 OR dm.IsHidden IS NULL)) AND (dm.IsDisabled = 0 OR dm.IsDisabled IS NULL)",
					new { departmentId = departmentId, retrieveHidden = retrieveHidden });

				return result.ToList();
			}
		}

		public List<IdentityUser> GetAllUsersCreatedAfterTimestamp(DateTime timestamp)
		{
			using (IDbConnection db = new SqlConnection(connectionString))
			{
				return db.Query<IdentityUser>(@"SELECT u.*, ue.* FROM AspNetUsers u
																				INNER JOIN AspNetUsersExt ue ON u.Id = ue.UserId
																				WHERE ue.CreateDate > @timestamp",
								new { timestamp = timestamp }).ToList();
			}
		}

		public List<UserGroupRole> GetAllUsersGroupsAndRoles(int departmentId, bool retrieveHidden, bool retrieveDisabled, bool retrieveDeleted)
		{
			using (IDbConnection db = new SqlConnection(connectionString))
			{
				return db.Query<UserGroupRole>(@"SELECT dgm.DepartmentGroupId, u.Id as 'UserId',
												(SELECT STUFF((
														SELECT ',' +  CONVERT(varchar, pru.PersonnelRoleId)
														FROM PersonnelRoleUsers pru
														WHERE pru.UserId = u.Id AND pru.DepartmentId = @departmentId
														FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 1, '')) as 'Roles',
												(SELECT STUFF((
														SELECT ', ' +  CONVERT(varchar, pr.Name)
														FROM PersonnelRoleUsers pru
														INNER JOIN PersonnelRoles pr ON pr.PersonnelRoleId = pru.PersonnelRoleId 
														WHERE pru.UserId = u.Id AND pru.DepartmentId = @departmentId
														FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 1, '')) as 'RoleNames',
												dg.Name as 'DepartmentGroupName',
												up.FirstName + ' ' + up.LastName AS 'Name'
												FROM DepartmentMembers dm
												INNER JOIN AspNetUsers u ON u.Id = dm.UserId
												LEFT OUTER JOIN DepartmentGroupMembers dgm ON u.Id = dgm.UserId
												LEFT OUTER JOIN DepartmentGroups dg ON dg.DepartmentGroupId = dgm.DepartmentGroupId
												INNER JOIN UserProfiles up ON up.UserId = u.Id
												WHERE dm.DepartmentId = @departmentId AND (@retrieveHidden = 1 OR (dm.IsHidden = 0 OR dm.IsHidden IS NULL)) AND (@retrieveDisabled = 1 OR (dm.IsDisabled = 0 OR dm.IsDisabled IS NULL)) AND (@retrieveDeleted = 1 OR (dm.IsDeleted = 0 OR dm.IsDeleted IS NULL))",
								new { departmentId = departmentId, retrieveHidden = retrieveHidden, retrieveDisabled = retrieveDisabled, retrieveDeleted = retrieveDeleted }).ToList();
			}
		}
	}
}