# BMP

A simple class to manage raw 24-bit BMP file format.

## Installation

Clone the [stable branch (master)](https://github.com/kolya5544/BMP/tree/master) and build BMP class to a .dll file or use its source code in your project.

## Usage

Import a .dll or source code to your project. Add this line to include the class from the library.

```csharp
using BMPC = BMP.BMP;
```

### Initialization

To initialize your work with BMP, use BMP's initalizer:

```csharp
//Simply initialize a new BMPC class
BMPC bmpc = new BMPC();
//Initialize a new BMPC class from a filename (can be PNG/JPG/most of extensions tbh)
BMPC bmpc = new BMPC(string filename);
//Initialize a new BMPC class from a System.Drawing.Bitmap
BMPC bmpc = new BMPC(System.Drawing.Bitmap bmp);
```

### Accessing and manipulating the contents

After initializing, you can change the contents of a BMP using one of those ways:

```csharp
//First way.
bmpc.Load("filename.png");
//Second way.
bmpc.Load(new System.Drawing.Bitmap(100,100));
//Third way.
bmpc.matrix[x, y] = 0x00RRGGBB;
//Fourth way.
bmpc.matrix = new int[100,100];
```

### Outputting the result

After manipulating the class, you can save the resulting bitmap as a raw (uncompressed) 24-bit BMP image.

You can define an `.ArbHeader` property of a class to manipulate 4 byte program-specific header (can be anything).

To get a `byte[]` of a resulting BMP file, use

```csharp
byte[] result = bmpc.GetContents();
```

Or, to save the file directly to the filesystem, use

```csharp
bmpc.Save("bitmap.bmp");
```

### Examples

```csharp
//Loads an image, changes colors a bit, and saved the picture.
Bitmap bmp = new Bitmap("example.png");
BMPC bmpc = new BMPC(bmp);

int A = 0;
for (int x = 0; x < bmpc.matrix.GetLength(0); x++){
	for (int y = 0; y < bmpc.matrix.GetLength(1); y++){
		A++;
		int color = bmpc.matrix[x,y];
		if (color + A > 0xFFFFFF) continue;
		color += A;
		bmpc.matrix[x,y] = color;
	}
}

bmpc.Save("output.bmp");
```

```csharp
//Loads an image, inverts all colors and saves the picture.

Bitmap bmp = new Bitmap("example.png");
BMPC bmpc = new BMPC(bmp);

for (int x = 0; x < bmpc.matrix.GetLength(0); x++){
	for (int y = 0; y < bmpc.matrix.GetLength(1); y++){
		int color = bmpc.matrix[x,y];
		color = 0xFFFFFF - color;
		bmpc.matrix[x,y] = color;
	}
}

bmpc.Save("output.bmp");
```

```csharp
//Creates a beautiful pattern

BMPC bmpc = new BMPC();
bmpc.matrix = new int[600, 600];

for (int x = 0; x < bmpc.matrix.GetLength(0); x++)
{
    for (int y = 0; y < bmpc.matrix.GetLength(1); y++)
    {
        int color = Math.Min(x*x*y*y, 0xFFFFFF);
        bmpc.matrix[x,y] = color;
    }
}

bmpc.Save("output.bmp");
```


## Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## License
[MIT](https://choosealicense.com/licenses/mit/)