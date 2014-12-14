<?php
/**
 * Created by PhpStorm.
 * User: Kessiler
 * Date: 14/12/2014
 * Time: 14:58
 */

$app->get('/', 'Core\Controller\IndexController::indexAction')->bind('index');