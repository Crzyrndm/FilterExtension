This is a collection of icons and configs contributed by users of the mod Filter Extensions.

============================  ICONS  ============================
Recommended size for icons is 32x32, but any image with dimensions between 25 and 40 pixels
in GameData is available to use. To make two state buttons, include a second image with the
"_selected" suffix, otherwise the second state will be automatically generated from the
first image.

Any icon with the same name as a sub-category that has no config (ie. all procedural sub
-categories) will be picked up by an icon auto-loader. Otherwise, use of a config will be
required to specify which sub-categories to use the icon with. Such a config uses the 
following syntax (duplicate title and oldTitle to only replace the icon):

SUBCATEGORY
{
	category = */Name of category goes here/*
	title = */Name of the subcategory as you want it to be shown/*
	oldTitle = */The name originally generated for the subCategory/*
	icon = */The image name that you wish to use as an icon/*
}

============================  CONFIGS  ============================
Configs can be used to create new categories, sub-categories, edit existing sub-categories, 
remove a sub-category, and to specify what parts go into each sub-category.

The basic syntax is shown above where it is only editing the icon and title of a sub-
category. To remove/delete a sub-category, you only need to remove the "title = " line. 
The sub-category specified by "oldTitle = " will be removed.

// Delete
SUBCATEGORY
{
	category = */Name of category goes here/*
	oldTitle = */The name originally generated for the subCategory/*
}