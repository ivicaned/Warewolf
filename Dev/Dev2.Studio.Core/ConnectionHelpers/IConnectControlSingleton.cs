/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.ObjectModel;

namespace Dev2.ConnectionHelpers
{
    public interface IConnectControlSingleton
    {
        void ToggleConnection(int selectedIndex);

        void Refresh(Guid environmentId);

        event EventHandler<ConnectionStatusChangedEventArg> ConnectedStatusChanged;
        event EventHandler<ConnectedServerChangedEvent> ConnectedServerChanged;
        ObservableCollection<IConnectControlEnvironment> Servers { get; set; }

        void ReloadServer();

        event EventHandler<ConnectedServerChangedEvent> AfterReload;
    }
}
