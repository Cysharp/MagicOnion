#!/bin/bash
binary="libgrpc.a"
extension=$(echo ${binary} | sed 's/.*\./\./') #.a
filename=$(basename ${binary} ${extension}) # libgrpc

# Architectures in the fat file: libgrpc.a are: armv7 x86_64 arm64 
set $(lipo -info libgrpc.a)
architectures=${*:8}
for arc in $architectures; do
    echo "start thin $arc from $binary"

    # extract architecture from binary.
    # libgrpc_arm64.a
    lipo $binary -thin $arc -output ${filename}_${arc}${extension}

    # strip debug symbol from binary
    #libgrpc_arm64_stripped.a
    #-S: remove debug symbol
    #-x: remove non global symbol
    strip -S -x -o ${filename}_${arc}_stripped${extension} -r ${filename}_${arc}${extension}

    # combine filename
    f="${f} ${filename}_${arc}_stripped${extension}"
done

# generate universal libgrpc.a again
echo generate universal lib from $f
lipo -create $f -output libgrpc_stripped.a

# clean up
for arc in $architectures; do
    rm ${filename}_${arc}${extension}
    rm ${filename}_${arc}_stripped${extension}
done