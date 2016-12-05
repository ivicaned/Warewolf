﻿Feature: SaveDialog
	In order to save services
	As a warewolf studio user
	I want to give the workflow a name and location

Scenario: SaveDialogServiceNameValidation
	Given The Warewolf Studio is running
	When I Click New Workflow Ribbon Button
	And I Drag Toolbox MultiAssign Onto DesignSurface
	And I Assign Value To Variable With Assign Tool Small View Row 1
	And I Click Save Ribbon Button to Open Save Dialog
	And I Remove WorkflowName From Save Dialog
	And I Enter Service Name Into Save Dialog As "TestingWF"
	And I Click SaveDialog Save Button
	And I Click Close Workflow Tab Button
	And I Click Duplicate From Explorer Context Menu for Service "TestingWF"
	And I Enter Service Name Into Duplicate Dialog As "TestingWF1"
	And I Enter Invalid Service Name Into Duplicate Dialog As "Inv@lid N&m#"
	And I Enter Invalid Service Name With Whitespace Into Duplicate Dialog As "Test "
	And I Enter Service Name Into Duplicate Dialog As "ValidWFName"
	And I Click Duplicate From Duplicate Dialog

Scenario: Create WorkFlow In Folder Opens Save Dialog With Folder Already Selected 
	Given I Filter the Explorer with "Unit Tests"
	And I RightClick Explorer Localhost First Item
	Then I Select NewWorkflow FromExplorerContextMenu
	And Unit Tests Url Exists
	And I Drag Toolbox MultiAssign Onto DesignSurface   
	And I Click Save Ribbon Button to Open Save Dialog

Scenario: Double Click Item In Save Dialog Does Not Open Resource
	Given I Click New Workflow Ribbon Button
	And I Drag Toolbox MultiAssign Onto DesignSurface
	Then I Click Save Ribbon Button to Open Save Dialog
	And I Filter Save Dialog Explorer with "Hello World"
	And I Double Click Resource On The Service Picker
	And I Click SaveDialog CancelButton
	Then Resource Does not Open

Scenario: Filter Save Dialog Close And ReOpen Clears The Filter
	Given I Click New Workflow Ribbon Button
	And I Drag Toolbox MultiAssign Onto DesignSurface
	Then I Click Save Ribbon Button to Open Save Dialog
	And I Filter Save Dialog Explorer with "Hello World"
	And I Click SaveDialog CancelButton
	Then Explorer Items appear on the Explorer Tree
	And I Click Save Ribbon Button to Open Save Dialog
	Then Explorer Items appear on the Save Dialog Explorer Tree

Scenario: Close Save Dialog Removes Explorer Filter
	Given I Click New Workflow Ribbon Button
	And I Drag Toolbox MultiAssign Onto DesignSurface
	Then I Click Save Ribbon Button to Open Save Dialog
	And I Filter Save Dialog Explorer with "Hello World"
	And I Click SaveDialog CancelButton
	Then Explorer Items appear on the Explorer Tree

Scenario: Rename Folder From Save Dialog
	Given I Click New Workflow Ribbon Button
	And I Drag Toolbox MultiAssign Onto DesignSurface
	Then I Click Save Ribbon Button to Open Save Dialog
	And I Filter Save Dialog Explorer with "FolderToRename"
	And I RightClick Save Dialog Localhost First Item
	And I Select Rename From SaveDialog ExplorerContextMenu
	And I Rename Save Dialog Explorer First Item To "FolderRenamed"
	And I Click SaveDialog CancelButton
	And I Filter the Explorer with "FolderRenamed"
	Then Explorer Contain Item "FolderRenamed"

Scenario: Create New Folder In Localhost Server From Save Dialog
	Given I Click New Workflow Ribbon Button
	And I Drag Toolbox MultiAssign Onto DesignSurface
	Then I Click Save Ribbon Button to Open Save Dialog
	And I RightClick Save Dialog Localhost
	And I Select New Folder From SaveDialog ExplorerContextMenu
	And I Name New Folder as "New Created Folder"
	And I Click SaveDialog CancelButton
	And I Filter the Explorer with "New Created Folder"
	Then Explorer Contain Item "New Created Folder"

Scenario: Create New Folder In Existing Folder As A Sub Folder From Save Dialog
	Given I Click New Workflow Ribbon Button
	And I Drag Toolbox MultiAssign Onto DesignSurface
	Then I Click Save Ribbon Button to Open Save Dialog
	And I Filter Save Dialog Explorer with "Acceptance Tests"
	And I RightClick Save Dialog Localhost First Item
	And I Select New Folder From SaveDialog ExplorerContextMenu
	And I Name New Folder as "New Created Sub Folder"
	And I Click SaveDialog CancelButton
	Then Explorer Contain Sub Item "New Created Sub Folder"

Scenario: Rename Resource From Save Dialog
	Given I Click New Workflow Ribbon Button
	And I Drag Toolbox MultiAssign Onto DesignSurface
	Then I Click Save Ribbon Button to Open Save Dialog
	And I Filter Save Dialog Explorer with "ResourceToRename"
	And I RightClick Save Dialog Localhost First Item
	And I Select Rename From SaveDialog ExplorerContextMenu
	And I Rename Save Dialog Explorer First Item To "ResourceRenamed"
	And I Click SaveDialog CancelButton
	And I Filter the Explorer with "ResourceRenamed"
	Then Explorer Contain Item "ResourceRenamed"

Scenario: Delete Resource From Save Dialog
	Given I Click New Workflow Ribbon Button
	And I Drag Toolbox MultiAssign Onto DesignSurface
	Then I Click Save Ribbon Button to Open Save Dialog
	And I Filter Save Dialog Explorer with "ResourceToDelete"
	And I RightClick Save Dialog Localhost First Item
	And I Select Delete From SaveDialog ExplorerContextMenu
	And I Click MessageBox Yes
	And I Filter the Explorer with "ResourceToDelete"
	Then Explorer Does Not Contain Item "ResourceToDelete"


Scenario: Move Resource To Localhost From Save Dialog
	Given I Click New Workflow Ribbon Button
	And I Drag Toolbox MultiAssign Onto DesignSurface
	Then I Click Save Ribbon Button to Open Save Dialog
	And I Filter Save Dialog Explorer with "ResourceToMove"
	And I Move resource to localhost
	And I Filter Save Dialog Explorer with "FolderToMove"
	Then Explorer Does Not Contain Item "ResourceToMove"

Scenario: Move Folder To FolderToRename From Save Dialog
	Given I Click New Workflow Ribbon Button
	And I Drag Toolbox MultiAssign Onto DesignSurface
	Then I Click Save Ribbon Button to Open Save Dialog
	And I Filter Save Dialog Explorer with "ResourceTo"
	And I Move FolderToMove into FolderToRename
	And I Filter Save Dialog Explorer with "FolderToMove"
	Then FolderToMove is child of FolderToRename

Scenario: Move Folder To Same Location From Save Dialog
	Given I Click New Workflow Ribbon Button
	And I Drag Toolbox MultiAssign Onto DesignSurface
	Then I Click Save Ribbon Button to Open Save Dialog
	And I Filter Save Dialog Explorer with "FolderToRename"
	And I Move FolderToRename into localhost
	And I Filter Save Dialog Explorer with "FolderToRename"
	Then FolderToMove is child of FolderToRename

