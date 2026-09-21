/**
 * This module contains base LSM Ebpg hooks abuse.
 * 
 * Note, i'm basing my ownership based on a custom UID for policy & simple poc testing but consider using more advanced techniques.
 */

#ifndef MOD_LSM_H
#define MOD_LSM_H

#include "../includes/bpf_config.h"
#include "../includes/bpf_common.h"
#include "../includes/bpf_structs.h"
#include "../includes/bpf_debug.h"
#include "../includes/bpf_events_handler.h"
#include "../data/bpf_events.h"
#include "../data/bpf_ringbuf.h"
#include "../lib/ftlib.h"

#endif