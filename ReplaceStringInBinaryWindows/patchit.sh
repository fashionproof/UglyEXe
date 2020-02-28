#!/bin/bash
# set -x

function patch_strings_in_file() {
    local FILE="$1"
    local PATTERN="$2"
    local REPLACEMENT="$3"	

    # Find all unique strings in FILE that contain the pattern 
    STRINGS=$(strings "${FILE}" | grep -o "${PATTERN}" | sort -u -r)
    echo "file '${FILE}'"

    if [ "${STRINGS}" != "" ] ; then
        echo "File '${FILE}' contain strings with '${PATTERN}' in them:"

        for OLD_STRING in "${STRINGS}" ; do
            # Create the new string with a simple bash-replacement
            NEW_STRING="${OLD_STRING//"${PATTERN}"/"${REPLACEMENT}"}"
	    #NEW_STRING="${REPLACEMENT}"

	    echo "old '${OLD_STRING}'"
	    echo "new '${NEW_STRING}'"


            # Create null terminated ASCII HEX representations of the strings
            # OLD_STRING_HEX="$(echo -n ${OLD_STRING} | xxd -g 0 -u -ps -c 256)00"
            # NEW_STRING_HEX="$(echo -n ${NEW_STRING} | xxd -g 0 -u -ps -c 256)00"
	    OLD_STRING_HEX="$(echo -n "${OLD_STRING}" | xxd -g 0 -u -ps -c 256 | tr -d '\n')"
	    NEW_STRING_HEX="$(echo -n "${NEW_STRING}" | xxd -g 0 -u -ps -c 256 | tr -d '\n')"


            if [ ${#NEW_STRING_HEX} -le ${#OLD_STRING_HEX} ] ; then
                # Pad the replacement string with null terminations so the
                # length matches the original string
                #while [ ${#NEW_STRING_HEX} -lt ${#OLD_STRING_HEX} ] ; do
                #    NEW_STRING_HEX="${NEW_STRING_HEX}00"
                #done



		echo "OLD HEX '${OLD_STRING_HEX}' NEW HEX '${NEW_STRING_HEX}'"
                # Now, replace every occurrence of OLD_STRING with NEW_STRING 
                echo -n "Replacing ${OLD_STRING} with ${NEW_STRING}... "
                hexdump -ve '1/1 "%.2X"' ${FILE} | \
                sed "s/"${OLD_STRING_HEX}"/"${NEW_STRING_HEX}"/g" | \
                xxd -r -p > ${FILE}.tmp
                chmod --reference ${FILE} ${FILE}.tmp
                cp ${FILE}.tmp ${FILE}
                echo "Done!"
            else
                echo "New string '${NEW_STRING}' is longer than old" \
                     "string '${OLD_STRING}'. Skipping."
            fi
        done
    fi
}


patch_strings_in_file mimihex2.exe 'mimi' 'fees'

patch_strings_in_file mimihex2.exe "Benjamin" "xxxxxxxx"
patch_strings_in_file mimihex2.exe "benjamin" "xxxxxxxx"
patch_strings_in_file mimihex2.exe "gentilkiwi" "genxxxxxxx"
patch_strings_in_file mimihex2.exe "kiwi" "kitt"
patch_strings_in_file mimihex2.exe "bd49a8271d650fa89e446b42e513b595a717b9212c91dd384aab871fc1d0f6d7" "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
patch_strings_in_file mimihex2.exe 'DELPY' 'fishy'
patch_strings_in_file mimihex2.exe 'log.gentilkiwi.com' 'fishgefishfishfish'
patch_strings_in_file mimihex2.exe 'Copyright' 'Copyrfish'
patch_strings_in_file mimihex2.exe '!This program cannot be run in DOS mode.' 'fishs program cannot be run in fishmfish'
patch_strings_in_file mimihex2.exe 'Unizeto Technologies' 'fisheto Technolofish'
patch_strings_in_file mimihex2.exe 'Certum' 'Cefish'
patch_strings_in_file mimihex2.exe 'crl.certum.pl' 'crl.certufish'
patch_strings_in_file mimihex2.exe 'ubca.ocsp-certum.com' 'fish.fish-cefish.com'
patch_strings_in_file mimihex2.exe 'repository.certum.pl/ctnca.cer' 'repositorfishrtufish/ctncafish'
patch_strings_in_file mimihex2.exe 'Open Source Developer' 'OfishSourfishevelfish'
patch_strings_in_file mimihex2.exe 'cscasha2.crl0q' 'cscafish.cfish'
patch_strings_in_file mimihex2.exe 'Build with love for POC only' 'fishd with fish forfish fish'
patch_strings_in_file mimihex2.exe 'Copyright (c) 2007 - 2019' 'Copyright (c) fish - fish'
patch_strings_in_file mimihex2.exe 'cscasha' 'cscfish'
patch_strings_in_file mimihex2.exe 'certum' 'cefish'
patch_strings_in_file mimihex2.exe 'Montreuil''Montrfish'


# new stuff
