#include "./Image.hpp"

namespace Tortuga
{
namespace Graphics
{
Image::Image()
{
    this->Width = 1;
    this->Height = 1;
    this->TotalByteSize = this->Width * this->Height * sizeof(glm::vec4);
    this->Pixels.resize(this->Width * this->Height);
    this->Pixels[0] = glm::vec4(0, 0, 0, 1);
}
Image::Image(uint32_t width, uint32_t height)
{
    this->Width = width;
    this->Height = height;
    this->TotalByteSize = this->Width * this->Height * sizeof(glm::vec4);
    this->Pixels.resize(this->Width * this->Height);
    this->Pixels[0] = glm::vec4(0, 0, 0, 1);
}
Image::Image(Utils::IO::ImageFile image)
{
    this->Width = image.Width;
    this->Height = image.Height;
    this->TotalByteSize = this->Width * this->Height * sizeof(glm::vec4);
    this->Pixels.resize(this->Width * this->Height);
    const float MAX_COLOR = 256.0f;
    for (uint32_t x = 0; x < this->Width; x++)
    {
        for (uint32_t y = 0; y < this->Height; y++)
        {
            uint32_t i = y * this->Width + x;
            uint32_t j = y * image.Pitch + x * image.BytesPerPixel;
            this->Pixels[i].r = 0;
            this->Pixels[i].g = 0;
            this->Pixels[i].b = 0;
            this->Pixels[i].a = 1;
            if (image.BytesPerPixel == 1)
            {
                this->Pixels[i].r = (float)image.Pixels[j + 0] / MAX_COLOR;
            }
            else if (image.BytesPerPixel == 2)
            {
                this->Pixels[i].r = (float)image.Pixels[j + 0] / MAX_COLOR;
                this->Pixels[i].g = (float)image.Pixels[j + 1] / MAX_COLOR;
            }
            else if (image.BytesPerPixel == 3)
            {
                this->Pixels[i].r = (float)image.Pixels[j + 0] / MAX_COLOR;
                this->Pixels[i].g = (float)image.Pixels[j + 1] / MAX_COLOR;
                this->Pixels[i].b = (float)image.Pixels[j + 2] / MAX_COLOR;
            }
            else if (image.BytesPerPixel == 4)
            {
                this->Pixels[i].r = (float)image.Pixels[j + 0] / MAX_COLOR;
                this->Pixels[i].g = (float)image.Pixels[j + 1] / MAX_COLOR;
                this->Pixels[i].b = (float)image.Pixels[j + 2] / MAX_COLOR;
                this->Pixels[i].a = (float)image.Pixels[j + 1] / MAX_COLOR;
            }
        }
    }
}

Image Image::Blue()
{
    auto data = Image();
    data.Width = 1;
    data.Height = 1;
    data.TotalByteSize = data.Width * data.Height * sizeof(glm::vec4);
    data.Pixels.resize(data.Width * data.Height);
    data.Pixels[0] = glm::vec4(0, 0, 1, 1);
    return data;
}
Image Image::White()
{
    auto data = Image();
    data.Width = 1;
    data.Height = 1;
    data.TotalByteSize = data.Width * data.Height * sizeof(glm::vec4);
    data.Pixels.resize(data.Width * data.Height);
    data.Pixels[0] = glm::vec4(1, 1, 1, 1);
    return data;
}

void Image::CopyChannel(Image sourceImage, ChannelType source, ChannelType destination)
{
    if (this->Width == sourceImage.Width && this->Height == sourceImage.Height)
    {
        //copy channel if image sizes match
        for (uint32_t i = 0; i < this->Pixels.size(); i++)
        {
            float val = 0.0f;
            if (source == CHANNEL_R)
                val = sourceImage.Pixels[i].r;
            else if (source == CHANNEL_G)
                val = sourceImage.Pixels[i].g;
            else if (source == CHANNEL_B)
                val = sourceImage.Pixels[i].b;
            else
            {
                Console::Error("passed unknown argument");
                return;
            }

            if (destination == CHANNEL_R)
                this->Pixels[i].r = val;
            else if (destination == CHANNEL_G)
                this->Pixels[i].g = val;
            else if (destination == CHANNEL_B)
                this->Pixels[i].b = val;
            {
                Console::Error("passed unknown argument");
                return;
            }
        }
    }
    else
    {
        //if image sizes are not exact then approximate
        for (uint32_t x = 0; x < this->Width; x++)
        {
            for (uint32_t y = 0; y < this->Height; y++)
            {
                uint32_t i = y * this->Width + x;
                uint32_t destX = glm::round(glm::min((sourceImage.Width / this->Width) * x, sourceImage.Width - 1));
                uint32_t destY = glm::round(glm::min((sourceImage.Height / this->Height) * y, sourceImage.Height - 1));
                uint32_t j = destY * sourceImage.Width + destX;

                float val = 0.0f;
                if (source == CHANNEL_R)
                    val = sourceImage.Pixels[j].r;
                else if (source == CHANNEL_G)
                    val = sourceImage.Pixels[j].g;
                else if (source == CHANNEL_B)
                    val = sourceImage.Pixels[j].b;
                else
                {
                    Console::Error("passed unknown argument");
                    return;
                }

                if (destination == CHANNEL_R)
                    this->Pixels[i].r = val;
                else if (destination == CHANNEL_G)
                    this->Pixels[i].g = val;
                else if (destination == CHANNEL_B)
                    this->Pixels[i].b = val;
                {
                    Console::Error("passed unknown argument");
                    return;
                }
            }
        }
    }
}
} // namespace Graphics
} // namespace Tortuga