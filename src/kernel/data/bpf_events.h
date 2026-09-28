/**
 * Contains utility for events types declarations
 * 
 * @author Yekuuun
 */

#ifndef BPF_EVENTS_H
#define BPF_EVENTS_H

#include "../includes/bpf_config.h"

#define MAX_EVENT_NAME 128
#define MAX_EVENT_DESC 512

//----------------------------------------------------
// ┌────────────────────────────────────┐
//  EVENT TYPES
// └────────────────────────────────────┘
//----------------------------------------------------
#define EVENT_DEBUG     1
#define EVENT_TRIGGERED 2

//----------------------------------------------------
// ┌────────────────────────────────────┐
//  PAYLOAD ENTITIES
// └────────────────────────────────────┘ 
//----------------------------------------------------
typedef struct ebpf_event_hdr {
    __u8  type;
    __u16 size;
    __u64 timestamp;
} __attribute__((packed)) ebpf_event_hdr;

typedef struct ebpf_event_payload {
    char event_name[MAX_EVENT_NAME];
    char event_desc[MAX_EVENT_DESC];
} __attribute__((packed)) ebpf_event_payload;

/**
 * Utility function for setting hdr attributes.
 */
#define SET_EVENT_HDR(_type, _size, _timestamp) \
((ebpf_event_hdr){                              \
    .type      = (_type),                       \
    .size      = (_size),                       \
    .timestamp = (_timestamp)                   \
})   

/**
 * Global event struct payload.
 */
typedef struct ebpf_event {
    struct ebpf_event_hdr hdr;
    struct ebpf_event_payload payload;
} __attribute__((packed)) ebpf_event;

#endif