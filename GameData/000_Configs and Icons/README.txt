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

// Edit
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

To create a new sub-category you also need to specify the conditions that will allow a
part to be visible in this sub-category. This is achieved through the use of FILTER and
CHECK nodes.

To be visible in a sub-category, a part must pass ALL Check's in ANY of the Filter's
** In Boolean terms, Checks are AND'd together, while Filter's are OR'd together

// Create
SUBCATEGORY
{
	category = Filter by Function
	title = Rover Wheel
	icon = R&D_node_icon_advancedmotors
	
	FILTER
	{
		// invert = false // This inverts the result of the AND'd Checks if true. So, if 
							it was true, and all CHECK nodes evaluate true, the filter
							would evaluate false, likewise, if any of the CHECK's
							evaluated false, this Filter would evaluate as true
		Check
		{
			type = moduleTitle
			value = Wheel
			// invert = false // invert has the default value "false", so you don't
							actually need to specify it. If invert was "true" then this
							CHECK would	pass all parts that DON'T have a wheel module
		}
	}
}

New Categories can also be created using config files. The colour key uses RGB or ARGB
hexadecimal codes (eg. #FFFF0000 for full red)

// New Category
CATEGORY
{
	title = */Category Title/*
	icon = */Category Icon/*
	colour = */Category Colour/*
}

There are also mod categories which have the stock Filter by Function categories generated
for a group of mods. Add multiple value entries seperated by comma's to include multiple
mods in the category

// Near Future Technologies Mod Category
CATEGORY
{
	title = Near Future Technologies
	icon = NearFuturePropulsion
	colour = #FFF0F0F0
	type = mod
	value = NearFuturePropulsion, NearFutureElectrical, NearFutureConstruction,
				NearFutureSolar, NearFutureSpacecraft
}