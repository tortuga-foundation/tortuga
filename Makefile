#executable name
TARGET = tortuga

#important paths
SRC_DIR = Source
OUT_DIR = Build
SRC_EXECUTABLE = Source/main.cpp

#compiler options
COMPILER = g++
PRE_PROCESSOR = -DDEBUG_MODE -DGLM_FORCE_RADIANS -DGLM_FORCE_DEPTH_ZERO_TO_ONE
FLAGS = -g -std=c++17 -pthread -Wall -Wno-narrowing -Wno-unused $(PRE_PROCESSOR) -fPIC
PATHS = -IBuild/include -LBuild/lib
LIBS = -lvulkan -lSDL2 -lSDL2_image -ldl

#get a list of all cpp files excluding executable file
SRC_FILES := $(shell find $(SRC_DIR)/ -type f -name '*.cpp' ! -path '$(SRC_EXECUTABLE)')
#get a list of all obj files
OBJ_FILES := $(patsubst $(SRC_DIR)/%.cpp,$(OUT_DIR)/%.o,$(SRC_FILES))

#link and create executable
all: $(OBJ_FILES)
	#static library
	ar rcs $(OUT_DIR)/lib/lib$(TARGET).a $(OBJ_FILES)
	#shared library
	$(COMPILER) -shared $(FLAGS) $(OBJ_FILES) -o $(OUT_DIR)/lib/lib$(TARGET).so $(PATHS) $(LIBS)
	#compile test application
	$(COMPILER) -o $(OUT_DIR)/$(TARGET) $(SRC_EXECUTABLE) $(FLAGS) $(PATHS) $(LIBS) -ltortuga

#create obj files
$(OUT_DIR)/%.o: $(SRC_DIR)/%.cpp
	@mkdir -p "$(@D)"
	$(COMPILER) -c -o $@ $< $(FLAGS) $(PATHS) $(LIBS)

clean:
	rm -rf $(OUT_DIR)/Tortuga
	rm -f $(OUT_DIR)/tortuga
	
init:
	#Check Prerequisites
	git --version
	cmake --version
	g++ --version
	make --version
	autoconf --version
	#init
	mkdir -p $(OUT_DIR)
	git submodule init
	git submodule update --recursive --init
	#vulkan headers
	mkdir -p Submodules/Vulkan-Headers/build
	cd Submodules/Vulkan-Headers/build && cmake -DCMAKE_INSTALL_PREFIX=$(PWD)/$(OUT_DIR) ..
	make install -C Submodules/Vulkan-Headers/build
	rm -rf Submodules/glslang/build
	#vulkan loader
	mkdir -p Submodules/Vulkan-Loader/build
	rm -rf $(PWD)/Submodules/Vulkan-Loader/build/helper.cmake
	echo 'set(VULKAN_HEADERS_INSTALL_DIR "$(PWD)/$(OUT_DIR)" CACHE STRING "" FORCE)' > $(PWD)/Submodules/Vulkan-Loader/build/helper.cmake
	cd Submodules/Vulkan-Loader/build && cmake -C helper.cmake -DCMAKE_INSTALL_PREFIX=$(PWD)/$(OUT_DIR) ..
	make -j4 -C Submodules/Vulkan-Loader/build
	make install -C Submodules/Vulkan-Loader/build
	rm -rf Submodules/Vulkan-Loader/build
	#SDL2
	cd Submodules/SDL && ./autogen.sh && ./configure --prefix=$(PWD)/$(OUT_DIR)
	make -j4 -C Submodules/SDL
	make install -C Submodules/SDL
	#SDL2 Image
	cd Submodules/SDL_image && ./autogen.sh && ./configure --prefix=$(PWD)/$(OUT_DIR)
	make -j4 -C Submodules/SDL_image
	make install -C Submodules/SDL_image
	#glm
	ln -f -s ../../Submodules/glm/glm $(PWD)/$(OUT_DIR)/include/glm
	#vulkan validation layers
	mkdir -p Submodules/Vulkan-ValidationLayers/build
	cd Submodules/Vulkan-ValidationLayers/build && python ../scripts/update_deps.py
	cp -r Submodules/Vulkan-ValidationLayers/build/glslang/build/install/* $(PWD)/$(OUT_DIR)/ 
	cd Submodules/Vulkan-ValidationLayers/build && cmake -DGLSLANG_INSTALL_DIR=$(PWD)/$(OUT_DIR) -DVULKAN_HEADERS_INSTALL_DIR=$(PWD)/$(OUT_DIR) -DCMAKE_INSTALL_PREFIX=$(PWD)/$(OUT_DIR) ..
	make -j4 -C Submodules/Vulkan-ValidationLayers/build
	make install -C Submodules/Vulkan-ValidationLayers/build
	rm -rf Submodules/Vulkan-ValidationLayers/build