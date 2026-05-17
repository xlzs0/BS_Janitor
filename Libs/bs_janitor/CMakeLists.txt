cmake_minimum_required(VERSION 3.14)

project(bs_janitor VERSION 1.0.0)

set(CMAKE_CXX_STANDARD 23)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

if((CMAKE_CXX_COMPILER_ID STREQUAL "GNU") OR (CMAKE_CXX_COMPILER_ID STREQUAL "Clang" AND CMAKE_CXX_COMPILER_FRONTEND_VARIANT STREQUAL "GNU"))
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -march=x86-64-v3 -Wall -Wextra -Wpedantic")
else()
    message(FATAL_ERROR "bs_janitor can only be compiled with Clang")
endif()

set(BS_JANITOR_SOURCES
    src/image.cc
    src/main.cc
    src/parser.cc
)

add_library(bs_janitor SHARED ${BS_JANITOR_SOURCES})
target_compile_features(bs_janitor PRIVATE cxx_std_23)
target_compile_definitions(bs_janitor PRIVATE _CRT_SECURE_NO_WARNINGS NOMINMAX)
target_include_directories(bs_janitor PUBLIC include)

include(FetchContent)

FetchContent_Declare(
    glaze
    GIT_REPOSITORY https://github.com/stephenberry/glaze.git
    GIT_TAG main
    GIT_SHALLOW TRUE
)
FetchContent_MakeAvailable(glaze)

target_link_libraries(bs_janitor PRIVATE glaze::glaze)

add_custom_command(TARGET bs_janitor POST_BUILD COMMAND ${CMAKE_COMMAND} -E copy "$<TARGET_FILE:bs_janitor>" "${CMAKE_SOURCE_DIR}/../")