cmake_minimum_required(VERSION 3.14)

project(bs_janitor VERSION 1.0.0)

if((CMAKE_CXX_COMPILER_ID STREQUAL "GNU") OR (CMAKE_CXX_COMPILER_ID STREQUAL "Clang" AND CMAKE_CXX_COMPILER_FRONTEND_VARIANT STREQUAL "GNU"))
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -Wall -Wextra -Wpedantic -Werror -march=x86-64-v3 -mpclmul")

    if(NOT WIN32)
        set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -fPIC")
    endif()
endif()

set(BS_JANITOR_SOURCES
    src/image.cc
    src/parser.cc
)

if(CMAKE_PROJECT_NAME STREQUAL PROJECT_NAME)
    add_library(bs_janitor SHARED ${BS_JANITOR_SOURCES})
else()
    add_library(bs_janitor STATIC ${BS_JANITOR_SOURCES})
endif()

target_compile_features(bs_janitor PRIVATE cxx_std_23)
target_include_directories(bs_janitor PUBLIC include)

include(FetchContent)

FetchContent_Declare(
    simdjson
    GIT_REPOSITORY https://github.com/simdjson/simdjson.git
    GIT_TAG v3.11.3
)

FetchContent_MakeAvailable(simdjson)

target_link_libraries(bs_janitor PRIVATE simdjson)

add_custom_command(
    TARGET bs_janitor POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E copy
        "$<TARGET_FILE:bs_janitor>"
        "${CMAKE_SOURCE_DIR}/../"
)