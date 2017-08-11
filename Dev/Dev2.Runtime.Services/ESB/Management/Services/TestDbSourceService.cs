﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class TestDbSourceService : IEsbManagementEndpoint
    {
        private readonly IDbSources _dbSources;

        public TestDbSourceService()
            : this(new DbSources())
        {

        }

        public TestDbSourceService(IDbSources dbSources)
        {
            _dbSources = dbSources;
        }
        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService()
        {
            return AuthorizationContext.Contribute;
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            ExecuteMessage msg = new ExecuteMessage();
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            try
            {

                Dev2Logger.Info("Test DB Connection Service", "Warewolf Info");
                StringBuilder resourceDefinition;

                values.TryGetValue("DbSource", out resourceDefinition);

                IDbSource src = serializer.Deserialize<DbSourceDefinition>(resourceDefinition);
                DatabaseValidationResult result = null;
                Common.Utilities.PerformActionInsideImpersonatedContext(Common.Utilities.OrginalExecutingUser, () =>
                {
                    result = _dbSources.DoDatabaseValidation(new DbSource
                    {
                        AuthenticationType = src.AuthenticationType,
                        Server = src.ServerName,
                        Password = src.Password,
                        ServerType = src.Type,
                        UserID = src.UserName

                    });

                });
                if (result == null)
                {
                    result = new DatabaseValidationResult { ErrorMessage = "Problem testing connection.", IsValid = false };
                }

                msg.Message = new StringBuilder(result.IsValid ? serializer.Serialize(result.DatabaseList) : result.ErrorMessage);
                msg.HasError = !result.IsValid;

            }
            catch (Exception err)
            {
                msg.HasError = true;
                msg.Message = new StringBuilder(err.Message);
                Dev2Logger.Error(err, "Warewolf Error");

            }

            return serializer.SerializeToBuilder(msg);
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService newDs = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Roles ColumnIODirection=\"Input\"/><DbSource ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };
            ServiceAction sa = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };
            newDs.Actions.Add(sa);

            return newDs;
        }

        public string HandlesType()
        {
            return "TestDbSourceService";
        }
    }
}
