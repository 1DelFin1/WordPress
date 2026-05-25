<?php
/**
 * Plugin Name: Inpad Objects
 * Description: Тип записи "Объекты" для ИНПАД с поддержкой REST API.
 * Version: 1.0
 */

add_action('init', function () {
    register_post_type('objects', [
        'labels' => [
            'name'          => 'Объекты',
            'singular_name' => 'Объект',
            'add_new_item'  => 'Добавить объект',
            'edit_item'     => 'Редактировать объект',
        ],
        'public'       => true,
        'show_in_rest' => true,
        'rest_base'    => 'objects',
        'supports'     => ['title', 'editor', 'excerpt', 'thumbnail', 'slug', 'custom-fields'],
        'has_archive'  => true,
        'rewrite'      => ['slug' => 'objects'],
        'menu_icon'    => 'dashicons-building',
    ]);
});

// Регистрация мета-полей — доступны через REST API в поле "meta"
add_action('init', function () {
    $fields = [
        'inpad_city'            => 'string',
        'inpad_object_type'     => 'string',
        'inpad_year_start'      => 'string',
        'inpad_year_end'        => 'string',
        'inpad_client'          => 'string',
        'inpad_role'            => 'string',
        'inpad_seo_title'       => 'string',
        'inpad_seo_description' => 'string',
    ];

    foreach ($fields as $key => $type) {
        register_post_meta('objects', $key, [
            'show_in_rest'  => true,
            'single'        => true,
            'type'          => $type,
            'auth_callback' => fn() => current_user_can('edit_posts'),
        ]);
    }
});