/**
 * Ringbuffer configuration.
 * 
 * NOTE : i'm using a ringbuffer config to log output events in the DEAMON. Consider it as useless for real offensive cases.
 * 
 * @author Yekuuun
 */

#ifndef BPF_RINGBUF_H
#define BPF_RINGBUF_H

#include "../includes/bpf_config.h"

#define MAX_RINGBUF_EVENTS 4096

/**
 * Main struct for handling output events using ring buffer.
 */
struct {
    __uint(type, BPF_MAP_TYPE_RINGBUF);
    __uint(max_entries, 4 * MAX_RINGBUF_EVENTS);
} event_output SEC(".maps");

#endif