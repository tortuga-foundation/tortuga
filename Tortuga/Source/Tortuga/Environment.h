#ifndef _ENVIRONMENT
#define _ENVIRONMENT

#include "Core.h"
#include "Entity.h"

namespace Tortuga
{
struct Environment
{
  std::vector<Entity *> Entities;
};
Environment *CreateEnvironment();
void DestroyEnvironment(Environment *env);

int32_t FindEntity(Environment *env, Entity *entity);
Entity *CreateEntity(Environment *env);
void DestroyEntity(Environment *env, Entity *entity);
}; // namespace Tortuga

#endif