<?php
/**
 * Created by PhpStorm.
 * User: Kessiler
 * Date: 14/12/2014
 * Time: 14:58
 */

$app = new Silex\Application();

/** SERVICES */
$app->register(new Silex\Provider\UrlGeneratorServiceProvider());
$app->register(new Silex\Provider\SessionServiceProvider());
$app->register(new Core\Service\ConfigServiceProvider(realpath(__DIR__) . "/../app/config/settings.json"));
$app->register(new Silex\Provider\DoctrineServiceProvider(), array('db.options' => $app['config']['database']));

$app['debug'] = $app['config']['debugger'];
$app->boot();

$app->register(new Silex\Provider\TwigServiceProvider(), array(
    'twig.path' => realpath(__DIR__) . '/../templates/' . $app['config']['template'],
    'twig.options' => array(
        'cache' => $app['config']['cache'] ? realpath(__DIR__) . "/../app/cache" : false,
        'strict_variables' => true
    )
));

/** ROUTING */
Symfony\Component\HttpFoundation\Request::enableHttpMethodParameterOverride();
require_once realpath(__DIR__) . "/routes.php";

return $app;