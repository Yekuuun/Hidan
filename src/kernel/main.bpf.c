/**
 * Main entry point for the kernel ebpf malicious module.
 * 
 * @ressources : 
 * https://syscalls64.paolostivanin.com/
 * 
 * @author Yekuuun
 */

#include "./includes/bpf_maps.h"
#include "./includes/bpf_config.h"
#include "./data/bpf_ringbuf.h"

//modules
#include "./modules/mod_hide_from.h"
#include "./modules/mod_test_evt.h"

/**
 * 
 * NOTE : All relevant hooks stands in dedicated .h files.
 * 
 */

char LICENSE[] SEC("license") = "GPL";
