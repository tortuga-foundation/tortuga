#executable name
TARGET = tortuga

#important paths
SRC_DIR = Source
OBJ_DIR = Build
SRC_EXECUTABLE = Source/main.cpp

#compiler options
COMPILER = g++
PRE_PROCESSOR = -DDEBUG_MODE -DGLM_FORCE_RADIANS -DGLM_FORCE_DEPTH_ZERO_TO_ONE
FLAGS = -g -std=c++17 -pthread -Wall -Wno-narrowing -Wno-unused $(PRE_PROCESSOR)
PATHS = -IBuild/include/ -LBuild/lib/
LIBS = -lvulkan -lSDL2 -lSDL2_image
LIBS_A := $(shell find $(OBJ_DIR)/lib/ -type f -name '*.a') $(shell find $(OBJ_DIR)/lib/ -type f -name '*.o')

#get a list of all cpp files excluding executable file
SRC_FILES := $(shell find $(SRC_DIR)/ -type f -name '*.cpp' ! -path '$(SRC_EXECUTABLE)')
#get a list of all obj files
OBJ_FILES := $(patsubst $(SRC_DIR)/%.cpp,$(OBJ_DIR)/%.o,$(SRC_FILES))

#link and create executable
all: $(OBJ_FILES)
	$(COMPILER) -o $(OBJ_DIR)/$(TARGET) $(SRC_EXECUTABLE) $(FLAGS) $(PATHS) $(LIBS) $(LIBS_A) $(OBJ_FILES)

#create obj files
$(OBJ_DIR)/%.o: $(SRC_DIR)/%.cpp
	@mkdir -p "$(@D)"
	$(COMPILER) -c -o $@ $< $(FLAGS) $(PATHS) $(LIBS)

clean:
	rm -rf $(OBJ_DIR)/Tortuga
	rm -f $(OBJ_DIR)/tortuga

comp:
	gcc -c -o $(OBJ_DIR)/lib/xdg-shell.a $(OBJ_DIR)/lib/xdg-shell.c -I$(OBJ_DIR)/include
	gcc -c -o $(OBJ_DIR)/lib/xdg-decoration.a $(OBJ_DIR)/lib/xdg-decoration.c -I$(OBJ_DIR)/include
	
init:
	mkdir -p $(OBJ_DIR)
	#init
	git submodule init
	git submodule update --recursive --init
	#vulkan headers
	mkdir -p Submodules/Vulkan-Headers/build
	cd Submodules/Vulkan-Headers/build && cmake -DCMAKE_INSTALL_PREFIX=$(PWD)/$(OBJ_DIR) ..
	make install -C Submodules/Vulkan-Headers/build
	rm -rf Submodules/glslang/build
	#vulkan loader
	mkdir -p Submodules/Vulkan-Loader/build
	rm -rf $(PWD)/Submodules/Vulkan-Loader/build/helper.cmake
	echo 'set(VULKAN_HEADERS_INSTALL_DIR "$(PWD)/$(OBJ_DIR)" CACHE STRING "" FORCE)' > $(PWD)/Submodules/Vulkan-Loader/build/helper.cmake
	cd Submodules/Vulkan-Loader/build && cmake -C helper.cmake -DCMAKE_INSTALL_PREFIX=$(PWD)/$(OBJ_DIR) ..
	make install -C Submodules/Vulkan-Loader/build
	rm -rf Submodules/Vulkan-Loader/build
	#SDL2
	cd Submodules/SDL/ && sh autogen.sh --prefix=$(PWD)/$(OBJ_DIR) && sh configure --prefix=$(PWD)/$(OBJ_DIR)
	make -C Submodules/SDL/
	make install -C Submodules/SDL/
	#SDL2 Image
	cd Submodules/SDL_image/ && sh autogen.sh --prefix=$(PWD)/$(OBJ_DIR) && sh configure --prefix=$(PWD)/$(OBJ_DIR)
	make -C Submodules/SDL_image/
	make install -C Submodules/SDL_image/
	#glm
	ln -f -s ../../Submodules/glm/glm $(OBJ_DIR)/include/glm
	#glslang
	mkdir -p Submodules/glslang/build
	cd Submodules/glslang/build && cmake -DCMAKE_BUILD_TYPE=Release -DCMAKE_INSTALL_PREFIX=$(PWD)/$(OBJ_DIR) ..
	make -C Submodules/glslang/build
	make install -C Submodules/glslang/build
	rm -rf Submodules/glslang/build
