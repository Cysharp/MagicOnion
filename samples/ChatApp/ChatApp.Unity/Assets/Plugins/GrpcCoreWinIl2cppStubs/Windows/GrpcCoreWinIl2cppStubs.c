// Dummy plugin to avoid errors when building with Windows IL2CPP
// Grpc.Core uses `[DllImport("__Internal")]` but Windows does not have `dlopen`, `dlerror` and `dlsym`, resulting in a build error.
//
// See: https://github.com/Cysharp/MagicOnion#workaround-for-il2cpp--windows-build-failure

#include <stdio.h>
#include <stdlib.h>

void* dlopen(const char* filename, int flags) {
  fprintf(stderr, "Should never reach here");
  abort();
}
char* dlerror(void) {
  fprintf(stderr, "Should never reach here");
  abort();
}
void* dlsym(void* handle, const char* symbol) {
  fprintf(stderr, "Should never reach here");
  abort();
}