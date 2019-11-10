![Build Status](https://github.com/tortuga-foundation/tortuga-library/workflows/Build/badge.svg)

# tortuga-library

Tortuga is a open source Game Engine built in C++ 17. Tortuga ONLY uses vulkan for its graphics API with NO plans of using any other graphics API. Tortuga is built for linux but should be compatible on Mac OS X and Windows.

## Key Features

* Multi-Threaded Rendering
* Entity Component System
* Event Based Input System
* Physically Based Rendering
* Data Oriented Design
* Garbage Collection
* Inline Shader Include
* Multi GPU Support (Split Screen) _[NOT IMPLIMENTED]_

## Prerequisites

Please make sure you are using the correct version of the software. Tortuga has been tested using the versions listed below.

* `git` version `2.23.0`
* `cmake` version `3.15.4`
* `gcc/g++` version `9.2.0`
* `GNU Make` version `4.2.1`
* `GNU bash` version `5.0.11`
* `Python` version `3.7.4`

## Building the library

##### Makefile

1. Download the project `git clone git@gitlab.com:zeeshan5951/tortuga-library.git`
2. Change Directory `cd tortuga-library`
3. Setup required dependencies `make init`
4. Build project `make -j4`
5. Run the application `LD_LIBRARY_PATH=$PWD/Build/lib ./Build/tortuga`

##### Visual Studio Code

1. Download the project `git clone git@gitlab.com:zeeshan5951/tortuga-library.git`
2. Open project with vs code
3. Run 'init' task to get and build submodules
4. Launch project
