#install dot net core 3.1
wget https://packages.microsoft.com/config/ubuntu/19.10/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install apt-transport-https
sudo apt-get update
sudo apt-get install dotnet-sdk-3.1
#install open al, sdl2, vulkan, bullet physics and glslang tools
sudo apt install -y libopenal1 libsdl2-2.0-0 libvulkan1 libbullet2.87 glslang-tools libgdiplus