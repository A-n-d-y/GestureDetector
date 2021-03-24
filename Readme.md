# Handwriting Recognition
Example Unity project using point-based gesture recognition using [PDollar](https://depts.washington.edu/acelab/proj/dollar/pdollar.html). Within this demo, you can draw a numeric value and it will detect the character, providing a confidence score.

![Demo](https://i.ibb.co/qF9fjs0/hr1-git.gif)
## Features

**Detect numeric gestures** - 
Draw a number within the Detection area numeric characters from 0 - 9. Once a gesture is detected, The draw area is replaced with the text label, and a confidence score is provided.

**Save new gestures** - 
You can create and train your gestures. Draw a gesture, provide it with a name and click save. The dataset will be saved in XML and is reloaded upon re-entering play mode.

**Set minimum confidence level** - 
A classification will only occur once the minimum confidence level is obtained on a scale of 0 - 1.

**Set expected gesture** - 
Provide an expected character within a text field, and the gesture will only classify if they are the same. If a different gesture has been detected, an "incorrect symbol" string is returned. For example, if you set the expected gesture to be "4" and the user inputs "5", it will correctly classify the gesture as the incorrect character.

**Compatible on multiple platforms** - 
The project works on iOS, Android, Mac, Windows and WebGL. (WebGL can not save gestures, as these are saved in a persistent path)

**Optional auto reset** - 
Gesture detection automatically resets after 2 seconds of successful detection.

**Event listeners** - 
Subscribe from any class to return when interaction begins or ends and when a gesture is matched.

# Files
```
Assets
	├── Plugins
		└── PDollar
	├── Resources
		└── GestureSet (XML data for datasets)
	├── Scenes 
		└── Demo scene
	└── Scripts
		└── GestureRecogniser.cs
		└── HandWritingRecognitionExample.cs
```

# Adding a new gesture
![Demo](https://i.ibb.co/swYcY1w/hr2-git.gif)

After creating a new gesture, the XML data will save in your persistent path. After reloading the app, it'll first load the data from within the Resources folder before checking the persistentData path.
After you have created new gestures, you should move the xml files from persistentData to your resources folder. Failure to do so will result in the gestures not being included in any builds. You will receive a debug log warning of this.

    Gesture /Users/.../C-132604898816961460.xml detected in persistentDataPath. Move to resources if required.

**Example XML File:**
```
<?xml version="1.0" encoding="utf-8" standalone="yes"?>
<Gesture Name = "0">
	<Stroke>
		<Point X = "1597" Y = "-629" T = "0" Pressure = "0" />
		<Point X = "1580" Y = "-614" T = "0" Pressure = "0" />
		<Point X = "1569" Y = "-594" T = "0" Pressure = "0" />
		<Point X = "1563" Y = "-570" T = "0" Pressure = "0" />
		...
	</Stroke>
</Gesture>
```

# Configuration
You can configure the defaults within GestureRecogniser.cs. As an example:

    public  float  recognitionDelay = 1; // How long to wait to count the next interaction as part of the same drawing

    public  float  minimumRequiredConfidenceScore = 0.90f; // Range: 0.0f - 1.0f
    
    public  bool  limitedArea = true; // Whether to constrain the space the user can draw to the box


# Future improvements
 - Save and load gestures to a remote server
 - Customisable save locations
 - Extend event listeners 

